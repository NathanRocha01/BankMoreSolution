using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Tests
{
    public static class CpfGenerator
    {
        private static readonly Random _random = new();

        public static string GerarCpfValido()
        {
            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var semente = new int[9];
            for (int i = 0; i < 9; i++)
                semente[i] = _random.Next(0, 9);

            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += semente[i] * multiplicador1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            int digito1 = resto;

            soma = 0;
            for (int i = 0; i < 9; i++)
                soma += semente[i] * multiplicador2[i];
            soma += digito1 * multiplicador2[9];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            int digito2 = resto;

            var cpf = string.Join("", semente) + digito1 + digito2;
            return cpf;
        }
    }
}
