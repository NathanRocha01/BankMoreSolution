using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Entities;
using Shared.Exceptions;
using Shared.Interface;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Transferencia.Application.Commands;
using Transferencia.Domain.Entities;
using Transferencia.Domain.Interfaces;

namespace Transferencia.Application.Handlers
{
    public class EfetuarTransferenciaCommandHandler : IRequestHandler<EfetuarTransferenciaCommand, Unit>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;
        private readonly ITransferenciaRepository _transferenciaRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly ITransferenciaLogRepository _logRepository;

        public EfetuarTransferenciaCommandHandler(
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache,
            ITransferenciaRepository transferenciaRepository,
            IIdempotenciaRepository idempotenciaRepository,
            ITransferenciaLogRepository logRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _transferenciaRepository = transferenciaRepository;
            _logRepository = logRepository;
            _idempotenciaRepository = idempotenciaRepository;
        }

        public async Task<Unit> Handle(EfetuarTransferenciaCommand request, CancellationToken _)
        {

            var requestJson = JsonSerializer.Serialize(request);

            var registro = await _idempotenciaRepository.ObterAsync(request.IdentificacaoRequisicao);
            if (registro != null)
            {
                if (!string.IsNullOrEmpty(registro.Resultado))
                    throw new DomainException($"Requisição já processada: {registro.Resultado}", "DUPLICATE_REQUEST");

                return Unit.Value;
            }
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var idContaOrigem = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(idContaOrigem))
                    throw new DomainException("Usuário não identificado.", "INVALID_TOKEN");

                var cacheKey = $"token:{idContaOrigem}";
                var tokenBytes = await _cache.GetAsync(cacheKey);

                if (tokenBytes == null || tokenBytes.Length == 0)
                    throw new DomainException("Token expirado ou ausente no cache.", "INVALID_TOKEN");

                var token = Encoding.UTF8.GetString(tokenBytes);
                if (request.Valor <= 0)
                    throw new DomainException("Valor inválido para transferência.", "INVALID_VALUE");

                var debitoOk = await _transferenciaRepository.DebitarAsync(token, request.Valor);
                if (!debitoOk)
                    throw new DomainException("Não foi possível debitar da conta de origem.", "INVALID_ACCOUNT");

                var creditoOk = await _transferenciaRepository.CreditarAsync(token, request.NumeroContaDestino, request.Valor);
                if (!creditoOk)
                {
                    await _transferenciaRepository.EstornarAsync(token, request.Valor);
                    throw new DomainException("Falha ao creditar na conta de destino. Estorno realizado.", "TRANSFER_FAILED");
                }

                var transferencia = new TransferenciaEntity
                {
                    IdContaCorrenteOrigem = idContaOrigem,
                    IdContaCorrenteDestino = request.NumeroContaDestino.ToString(),
                    Valor = request.Valor,
                    DataMovimento = DateTime.UtcNow
                };

                await _logRepository.CriarAsync(transferencia);

                return Unit.Value;
            }
            catch (DomainException ex)
            {
                var idem = new Idempotencia
                {
                    ChaveIdempotencia = request.IdentificacaoRequisicao,
                    Requisicao = requestJson,
                    Resultado = $"{ex.ErrorCode}: {ex.Message}"
                };
                await _idempotenciaRepository.CriarAsync(idem);

                throw;
            }
        }
    }
}
