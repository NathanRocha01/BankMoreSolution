using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Domain.Entities
{
    public class Conta
    {
        public string IdContaCorrente { get; set; } = Guid.NewGuid().ToString();
        public int Numero { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public bool Ativo { get; set; } = false;
        public string Senha { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
    }
}
