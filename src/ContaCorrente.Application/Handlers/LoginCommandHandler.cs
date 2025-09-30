using ContaCorrente.Application.Commands;
using ContaCorrente.Domain.Interface;
using ContaCorrente.Infrastructure.Security;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ContaCorrente.Application.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
    {
        private readonly IContaRepository _contaRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IDistributedCache _cache;

        public LoginCommandHandler(
            IContaRepository contaRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IDistributedCache cache)
        {
            _contaRepository = contaRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _cache = cache;
        }

        public async Task<string> Handle(LoginCommand request, CancellationToken _)
        {
            var conta = request.NumeroConta.HasValue
                ? await _contaRepository.ObterPorNumeroAsync(request.NumeroConta.Value)
                : await _contaRepository.ObterPorCpfAsync(request.Cpf!);

            if (conta == null)
                throw new DomainException("Usuário não encontrado.", "USER_UNAUTHORIZED");

            var senhaHash = HashSenha(request.Senha, conta.Salt);
            if (conta.Senha != senhaHash)
                throw new DomainException("Senha inválida.", "USER_UNAUTHORIZED");

            var token = _jwtTokenGenerator.GerarToken(conta.IdContaCorrente);

            // Salva o token no cache
            var cacheKey = $"token:{conta.IdContaCorrente}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // mesmo tempo de expiração do JWT
            };
            await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(token), options);

            return token;
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
