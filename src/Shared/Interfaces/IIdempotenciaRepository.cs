using Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interface
{
    public interface IIdempotenciaRepository
    {
        Task<Idempotencia?> ObterAsync(string chaveIdempotencia);
        Task CriarAsync(Idempotencia registro);
    }
}
