using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.DTOs
{
    public class SaldoResponse
    {
        public int NumeroConta { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime DataHoraConsulta { get; set; } = DateTime.UtcNow;
        public decimal ValorSaldo { get; set; }
    }
}
