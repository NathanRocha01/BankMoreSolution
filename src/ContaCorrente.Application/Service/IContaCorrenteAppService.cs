using ContaCorrente.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Service
{
    public interface IContaCorrenteAppService
    {
        Task EfetuarMovimentacaoAsync(EfetuarMovimentacaoCommand cmd, string? actorContaId, bool isSystem, CancellationToken ct);
        Task DebitarTarifaAsync(string contaId, decimal valor, string idempotencyKey, CancellationToken ct);
    }
}
