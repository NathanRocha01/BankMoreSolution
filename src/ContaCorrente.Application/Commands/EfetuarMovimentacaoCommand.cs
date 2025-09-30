using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Commands
{
    public class EfetuarMovimentacaoCommand : IRequest<Unit>
    {
        public int? NumeroConta { get; set; }
        public decimal Valor { get; set; }
        public char TipoMovimento { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
