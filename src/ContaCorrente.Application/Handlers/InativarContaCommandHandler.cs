using ContaCorrente.Application.Commands;
using ContaCorrente.Domain.Interface;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Handlers
{
    public class InativarContaCommandHandler : IRequestHandler<InativarContaCommand, Unit>
    {
        private readonly IContaRepository _contaRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InativarContaCommandHandler(
            IContaRepository contaRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _contaRepository = contaRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Unit> Handle(InativarContaCommand request, CancellationToken _)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var idConta = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idConta))
                throw new DomainException("Usuário não identificado.", "INVALID_TOKEN");

            var conta = await _contaRepository.ObterPorIdAsync(idConta);
            if (conta == null)
                throw new DomainException("Conta não encontrada.", "INVALID_ACCOUNT");

            var senhaHash = HashSenha(request.Senha, conta.Salt);
            if (conta.Senha != senhaHash)
                throw new DomainException("Senha inválida.", "USER_UNAUTHORIZED");

            conta.Ativo = false;
            await _contaRepository.AtualizarAsync(conta);

            return Unit.Value;
        }

        private string HashSenha(string senha, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(senha + salt);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
