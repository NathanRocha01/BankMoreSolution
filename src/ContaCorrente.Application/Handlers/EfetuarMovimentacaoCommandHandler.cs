using ContaCorrente.Application.Commands;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interface;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Entities;
using Shared.Exceptions;
using Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Handlers
{
    public class EfetuarMovimentacaoCommandHandler : IRequestHandler<EfetuarMovimentacaoCommand, Unit>
    {
        private readonly IContaRepository _contaRepository;
        private readonly IMovimentoRepository _movimentoRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EfetuarMovimentacaoCommandHandler(
            IHttpContextAccessor httpContextAccessor,
            IContaRepository contaRepository,
            IMovimentoRepository movimentoRepository,
            IIdempotenciaRepository idempotenciaRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _contaRepository = contaRepository;
            _movimentoRepository = movimentoRepository;
            _idempotenciaRepository = idempotenciaRepository;
        }

        public async Task<Unit> Handle(EfetuarMovimentacaoCommand request, CancellationToken _)
        {
            var requestJson = JsonSerializer.Serialize(request);

            var registro = await _idempotenciaRepository.ObterAsync(request.IdempotencyKey);
            if (registro != null)
            {
                if (!string.IsNullOrEmpty(registro.Resultado))
                    throw new DomainException($"Requisição já processada: {registro.Resultado}", "DUPLICATE_REQUEST");

                return Unit.Value;
            }

            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var idContaAutenticada = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(idContaAutenticada))
                    throw new DomainException("Usuário não identificado.", "INVALID_TOKEN");

                Conta? conta = null;

                if (request.NumeroConta.HasValue)
                {
                    conta = await _contaRepository.ObterPorNumeroAsync(request.NumeroConta.Value);
                }
                else
                {
                    conta = await _contaRepository.ObterPorIdAsync(idContaAutenticada);
                }

                if (conta == null)
                    throw new DomainException("Conta corrente não encontrada.", "INVALID_ACCOUNT");

                if (!conta.Ativo)
                    throw new DomainException("Conta corrente está inativa.", "INACTIVE_ACCOUNT");

                if (request.Valor <= 0)
                    throw new DomainException("Valor deve ser positivo.", "INVALID_VALUE");

                if (request.TipoMovimento != 'C' && request.TipoMovimento != 'D')
                    throw new DomainException("Tipo de movimento inválido.", "INVALID_TYPE");

                if (request.NumeroConta.HasValue &&
                    conta.IdContaCorrente != idContaAutenticada &&
                    request.TipoMovimento != 'C')
                {
                    throw new DomainException("Somente crédito permitido em contas de terceiros.", "INVALID_TYPE");
                }

                var movimento = new Movimento
                {
                    IdMovimento = Guid.NewGuid().ToString(),
                    IdContaCorrente = conta.IdContaCorrente,
                    DataMovimento = DateTime.UtcNow,
                    TipoMovimento = request.TipoMovimento,
                    Valor = request.Valor
                };

                await _movimentoRepository.CriarAsync(movimento);

                var idem = new Idempotencia
                {
                    ChaveIdempotencia = request.IdempotencyKey,
                    Requisicao = requestJson,
                    Resultado = "SUCCESS"
                };

                await _idempotenciaRepository.CriarAsync(idem);

                return Unit.Value;

            }catch (DomainException ex)
            {
                var idem = new Idempotencia
                {
                    ChaveIdempotencia = request.IdempotencyKey,
                    Requisicao = requestJson,
                    Resultado = $"{ex.ErrorCode}: {ex.Message}"
                };
                await _idempotenciaRepository.CriarAsync(idem);

                throw;
            }
        }
    }
}
