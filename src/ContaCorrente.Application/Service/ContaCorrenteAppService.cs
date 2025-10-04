using ContaCorrente.Application.Commands;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interface;
using Shared.Entities;
using Shared.Exceptions;
using Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Service
{
    public sealed class ContaCorrenteAppService : IContaCorrenteAppService
    {
        private readonly IContaRepository _contaRepository;
        private readonly IMovimentoRepository _movimentoRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;

        public ContaCorrenteAppService(
            IContaRepository contaRepository,
            IMovimentoRepository movimentoRepository,
            IIdempotenciaRepository idempotenciaRepository)
        {
            _contaRepository = contaRepository;
            _movimentoRepository = movimentoRepository;
            _idempotenciaRepository = idempotenciaRepository;
        }

        public async Task EfetuarMovimentacaoAsync(
            EfetuarMovimentacaoCommand request,
            string? actorContaId,
            bool isSystem,
            CancellationToken ct)
        {
            var requestJson = JsonSerializer.Serialize(request);

            // Idempotência
            var registro = await _idempotenciaRepository.ObterAsync(request.IdempotencyKey);
            if (registro is not null)
            {
                if (!string.IsNullOrEmpty(registro.Resultado))
                    throw new DomainException($"Requisição já processada: {registro.Resultado}", "DUPLICATE_REQUEST");
                return;
            }

            try
            {
                // Descobrir conta alvo
                string? contaRefId = null;

                if (request.NumeroConta.HasValue)
                {
                    var contaPorNumero = await _contaRepository.ObterPorNumeroAsync(request.NumeroConta.Value);
                    contaRefId = contaPorNumero?.IdContaCorrente;
                    if (contaPorNumero is null)
                        throw new DomainException("Conta corrente não encontrada.", "INVALID_ACCOUNT");

                    await ValidarContaAtiva(contaPorNumero);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(actorContaId) && !isSystem)
                        throw new DomainException("Usuário não identificado.", "INVALID_TOKEN");

                    var contaPorId = await _contaRepository.ObterPorIdAsync(actorContaId!);
                    contaRefId = contaPorId?.IdContaCorrente;
                    if (contaPorId is null)
                        throw new DomainException("Conta corrente não encontrada.", "INVALID_ACCOUNT");

                    await ValidarContaAtiva(contaPorId);
                }

                // Regras de valor/tipo
                if (request.Valor <= 0)
                    throw new DomainException("Valor deve ser positivo.", "INVALID_VALUE");

                if (request.TipoMovimento != 'C' && request.TipoMovimento != 'D')
                    throw new DomainException("Tipo de movimento inválido.", "INVALID_TYPE");

                // Regra de terceiros (somente crédito), exceto chamadas internas (sistema)
                if (!isSystem &&
                    request.NumeroConta.HasValue &&
                    contaRefId != actorContaId &&
                    request.TipoMovimento != 'C')
                {
                    throw new DomainException("Somente crédito permitido em contas de terceiros.", "INVALID_TYPE");
                }

                // Persistir movimento
                var mov = new Movimento
                {
                    IdMovimento = Guid.NewGuid().ToString(),
                    IdContaCorrente = contaRefId!,
                    DataMovimento = DateTime.UtcNow,
                    TipoMovimento = request.TipoMovimento,
                    Valor = request.Valor
                };

                await _movimentoRepository.CriarAsync(mov);

                await _idempotenciaRepository.CriarAsync(new Idempotencia
                {
                    ChaveIdempotencia = request.IdempotencyKey,
                    Requisicao = requestJson,
                    Resultado = "SUCCESS"
                });
            }
            catch (DomainException ex)
            {
                await _idempotenciaRepository.CriarAsync(new Idempotencia
                {
                    ChaveIdempotencia = request.IdempotencyKey,
                    Requisicao = requestJson,
                    Resultado = $"{ex.ErrorCode}: {ex.Message}"
                });
                throw;
            }
        }

        public Task DebitarTarifaAsync(string contaId, decimal valor, string idempotencyKey, CancellationToken ct)
        {
            var cmd = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = idempotencyKey,
                NumeroConta = null,          // operar por Id
                TipoMovimento = 'D',         // débito de tarifa
                Valor = valor
            };

            // chamada interna (isSystem = true), actor = contaId
            return EfetuarMovimentacaoAsync(cmd, contaId, isSystem: true, ct);
        }

        private static Task ValidarContaAtiva(Conta conta)
        {
            if (!conta.Ativo)
                throw new DomainException("Conta corrente está inativa.", "INACTIVE_ACCOUNT");
            return Task.CompletedTask;
        }
    }
}
