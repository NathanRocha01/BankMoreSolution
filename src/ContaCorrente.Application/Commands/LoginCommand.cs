using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Commands
{
    public class LoginCommand : IRequest<string> // Retorna o token JWT
    {
        public int? NumeroConta { get; set; } // Pode ser nulo se usar CPF
        public string? Cpf { get; set; } // Pode ser nulo se usar número da conta
        public string Senha { get; set; } = string.Empty;
    }
}
