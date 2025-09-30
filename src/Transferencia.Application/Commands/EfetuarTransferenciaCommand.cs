using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Transferencia.Application.Commands
{
    public class EfetuarTransferenciaCommand : IRequest<Unit>
    {
        public int NumeroContaDestino { get; set; }
        public decimal Valor { get; set; }
        public string IdentificacaoRequisicao { get; set; } = string.Empty;
    }
}
