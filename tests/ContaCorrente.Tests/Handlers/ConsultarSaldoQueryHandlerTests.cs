using ContaCorrente.Application.Handlers;
using ContaCorrente.Application.Queries;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interface;
using ContaCorrente.Tests.Fixtures;
using FluentAssertions;
using Moq;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Tests.Handlers
{
    public class ConsultarSaldoQueryHandlerTests
    {
        private readonly Mock<IContaRepository> _contaRepositoryMock = new();
        private readonly Mock<IMovimentoRepository> _movimentoRepositoryMock = new();
        private readonly ConsultarSaldoQueryHandler _handler;

        public ConsultarSaldoQueryHandlerTests()
        {
            _handler = new ConsultarSaldoQueryHandler(
                _contaRepositoryMock.Object,
                _movimentoRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_DeveRetornarSaldoCorreto_QuandoContaComMovimentacoes()
        {
            // Arrange
            var conta = ContaFixture.CriarContaValida();
            conta.Nome = "Usuário Teste";
            conta.Numero = 12345;

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(conta.IdContaCorrente))
                .ReturnsAsync(conta);

            var movimentos = new List<Movimento>
        {
            new Movimento { TipoMovimento = 'C', Valor = 200 },
            new Movimento { TipoMovimento = 'D', Valor = 50 },
            new Movimento { TipoMovimento = 'C', Valor = 100 },
        };

            _movimentoRepositoryMock.Setup(x => x.ObterPorContaAsync(conta.IdContaCorrente))
                .ReturnsAsync(movimentos);

            var query = new ConsultarSaldoQuery { IdContaCorrente = conta.IdContaCorrente };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.NumeroConta.Should().Be(12345);
            result.Nome.Should().Be("Usuário Teste");
            result.ValorSaldo.Should().Be(250); // 200 + 100 - 50
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoContaNaoEncontrada()
        {
            // Arrange
            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync("idInexistente"))
                .ReturnsAsync((Conta?)null);

            var query = new ConsultarSaldoQuery { IdContaCorrente = "idInexistente" };

            // Act
            var act = () => _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Conta corrente não encontrada.*")
                .Where(e => e.ErrorCode == "INVALID_ACCOUNT");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoContaInativa()
        {
            var conta = ContaFixture.CriarContaValida();
            conta.Ativo = false;

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(conta.IdContaCorrente))
                .ReturnsAsync(conta);

            var query = new ConsultarSaldoQuery { IdContaCorrente = conta.IdContaCorrente };

            var act = () => _handler.Handle(query, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Conta corrente está inativa.*")
                .Where(e => e.ErrorCode == "INACTIVE_ACCOUNT");
        }

        [Fact]
        public async Task Handle_DeveRetornarSaldoZero_QuandoNaoExistemMovimentacoes()
        {
            var conta = ContaFixture.CriarContaValida();
            conta.Nome = "Fulano";
            conta.Numero = 77777;

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(conta.IdContaCorrente))
                .ReturnsAsync(conta);

            _movimentoRepositoryMock.Setup(x => x.ObterPorContaAsync(conta.IdContaCorrente))
                .ReturnsAsync(new List<Movimento>());

            var query = new ConsultarSaldoQuery { IdContaCorrente = conta.IdContaCorrente };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.ValorSaldo.Should().Be(0);
            result.NumeroConta.Should().Be(77777);
            result.Nome.Should().Be("Fulano");
        }
    }
}
