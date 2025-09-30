using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContaCorrente.Domain.Entities;
using System.Security.Cryptography;

namespace ContaCorrente.Tests.Fixtures
{
    public static class ContaFixture
    {
        public static Conta CriarContaValida(string senhaOriginal = "senha123")
        {
            var salt = "salt123";
            var senhaHash = HashSenha(senhaOriginal, salt);

            return new Conta
            {
                IdContaCorrente = Guid.NewGuid().ToString(),
                Nome = "Silva da Silva",
                Cpf = "49631509052",
                Numero = 123,
                Salt = salt,
                Senha = senhaHash,
                Ativo = true
            };
        }

        private static string HashSenha(string senha, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(senha + salt);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
