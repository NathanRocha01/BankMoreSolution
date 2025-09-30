using ContaCorrente.Application.Commands;
using Shared.Exceptions;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Extensions;
using ContaCorrente.Domain.Interface;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Handlers
{
    public class CriarContaCommandHandler : IRequestHandler<CriarContaCommand, int>
    {
        private readonly IContaRepository _contaRepository;

        public CriarContaCommandHandler(IContaRepository contaRepository)
        {
            _contaRepository = contaRepository;
        }

        public async Task<int> Handle(CriarContaCommand request, CancellationToken _)
        {
            if (!request.Cpf.EhCpfValido())
                throw new DomainException("CPF inválido.", "INVALID_DOCUMENT");

            var salt = Guid.NewGuid().ToString("N");
            var senhaHash = HashSenha(request.Senha, salt);

            var conta = new Conta
            {
                Nome = request.Nome,
                Cpf = request.Cpf,
                Salt = salt,
                Senha = senhaHash,
                Ativo = true
            };

            int numeroConta;
            var rand = new Random();
            do
            {
                numeroConta = rand.Next(100000, 999999);
            } while (await _contaRepository.VerificarExistenciaNumeroAsync(numeroConta));

            conta.Numero = numeroConta;

            await _contaRepository.CriarAsync(conta);

            return conta.Numero;
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
