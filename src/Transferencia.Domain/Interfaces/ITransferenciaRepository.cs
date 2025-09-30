using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transferencia.Domain.Interfaces
{
    public interface ITransferenciaRepository
    {
        Task<bool> DebitarAsync(string token, decimal valor);
        Task<bool> CreditarAsync(string token, int numeroContaDestino, decimal valor);
        Task<bool> EstornarAsync(string token, decimal valor);
    }
}
