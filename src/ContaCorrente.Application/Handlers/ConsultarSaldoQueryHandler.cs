using ContaCorrente.Application.DTOs;
using Shared.Exceptions;
using ContaCorrente.Application.Queries;
using ContaCorrente.Domain.Interface;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Handlers
{
    public class ConsultarSaldoQueryHandler : IRequestHandler<ConsultarSaldoQuery, SaldoResponse>
    {
        private readonly IContaRepository _contaRepository;
        private readonly IMovimentoRepository _movimentoRepository;

        public ConsultarSaldoQueryHandler(
            IContaRepository contaRepository,
            IMovimentoRepository movimentoRepository)
        {
            _contaRepository = contaRepository;
            _movimentoRepository = movimentoRepository;
        }

        public async Task<SaldoResponse> Handle(ConsultarSaldoQuery request, CancellationToken _)
        {
            var conta = await _contaRepository.ObterPorIdAsync(request.IdContaCorrente);

            if (conta == null)
                throw new DomainException("Conta corrente não encontrada.", "INVALID_ACCOUNT");

            if (!conta.Ativo)
                throw new DomainException("Conta corrente está inativa.", "INACTIVE_ACCOUNT");

            var movimentos = await _movimentoRepository.ObterPorContaAsync(conta.IdContaCorrente);

            var totalCredito = movimentos
                .Where(m => m.TipoMovimento == 'C')
                .Sum(m => m.Valor);

            var totalDebito = movimentos
                .Where(m => m.TipoMovimento == 'D')
                .Sum(m => m.Valor);

            var saldo = totalCredito - totalDebito;

            return new SaldoResponse
            {
                NumeroConta = conta.Numero,
                Nome = conta.Nome,
                DataHoraConsulta = DateTime.UtcNow,
                ValorSaldo = saldo
            };
        }
    }
}
