using ContaCorrente.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Domain.Interface
{
    public interface IMovimentoRepository
    {
        Task<IEnumerable<Movimento>> ObterPorContaAsync(string idContaCorrente);
        Task CriarAsync(Movimento movimento);
    }
}
