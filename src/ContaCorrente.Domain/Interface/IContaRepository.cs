using ContaCorrente.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Domain.Interface
{
    public interface IContaRepository
    {
        Task<Conta?> ObterPorNumeroAsync(int numero);
        Task<bool> VerificarExistenciaNumeroAsync(int numero);
        Task CriarAsync(Conta conta);
        Task AtualizarAsync(Conta conta);
        Task<Conta?> ObterPorCpfAsync(string cpf);
        Task<Conta?> ObterPorIdAsync(string id);
    }
}
