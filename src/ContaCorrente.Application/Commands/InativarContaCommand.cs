using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Commands
{
    public class InativarContaCommand : IRequest<Unit> // Retorna 204 No Content
    {
        public string Senha { get; set; } = string.Empty;
    }
}
