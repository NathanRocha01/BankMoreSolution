using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transferencia.Domain.Entities;

namespace Transferencia.Domain.Interfaces
{
    public interface ITransferenciaLogRepository
    {
        Task CriarAsync(TransferenciaEntity transferencia);
    }
}
