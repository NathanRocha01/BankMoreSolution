using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transferencia.Domain.Entities
{
    public class TransferenciaEntity
    {
        public string IdTransferencia { get; set; } = Guid.NewGuid().ToString();
        public string IdContaCorrenteOrigem { get; set; } = string.Empty;
        public string IdContaCorrenteDestino { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataMovimento { get; set; } = DateTime.UtcNow;
    }
}
