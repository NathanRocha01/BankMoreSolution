using ContaCorrente.Application.Commands;
using ContaCorrente.Application.Service;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interface;
using Moq;
using Shared.Entities;
using Shared.Exceptions;
using Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Tests.Services
{
    public class ContaCorrenteAppServiceTests
    {
        private readonly Mock<IContaRepository> _mockContaRepository = new();
        private readonly Mock<IMovimentoRepository> _mockMovimentoRepository = new();
        private readonly Mock<IIdempotenciaRepository> _mockIdempotenciaRepository = new();
        private readonly ContaCorrenteAppService _service;

        public ContaCorrenteAppServiceTests()
        {
            _service = new ContaCorrenteAppService(
                _mockContaRepository.Object,
                _mockMovimentoRepository.Object,
                _mockIdempotenciaRepository.Object
            );
        }

        [Fact]
        public async Task EfetuarMovimentacaoAsync_WhenAlreadyProcessed_WithResultadoNotEmpty_ShouldThrowDomainException()
        {
            // Arrange
            var key = "abc";
            _mockIdempotenciaRepository.Setup(r => r.ObterAsync(key))
                .ReturnsAsync(new Idempotencia { ChaveIdempotencia = key, Resultado = "SOME" });

            var cmd = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = key,
                NumeroConta = 1,
                TipoMovimento = 'C',
                Valor = 100
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                _service.EfetuarMovimentacaoAsync(cmd, actorContaId: "some", isSystem: false, CancellationToken.None));

            Assert.Contains("Requisição já processada", ex.Message);

            // Também garante que não persistiu movimento
            _mockMovimentoRepository.Verify(r => r.CriarAsync(It.IsAny<Movimento>()), Times.Never);
        }

        [Fact]
        public async Task EfetuarMovimentacaoAsync_WhenContaNotFound_ShouldThrowDomainException()
        {
            // Arrange
            var key = "key1";
            _mockIdempotenciaRepository.Setup(r => r.ObterAsync(key))
                .ReturnsAsync((Idempotencia?)null);

            _mockContaRepository.Setup(r => r.ObterPorNumeroAsync(10))
                .ReturnsAsync((Conta?)null);

            var cmd = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = key,
                NumeroConta = 10,
                TipoMovimento = 'C',
                Valor = 50
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                _service.EfetuarMovimentacaoAsync(cmd, actorContaId: null, isSystem: false, CancellationToken.None));

            Assert.Contains("Conta corrente não encontrada", ex.Message);
        }

        [Fact]
        public async Task EfetuarMovimentacaoAsync_WhenValorNegative_ShouldThrowDomainException()
        {
            // Arrange
            var key = "k2";
            _mockIdempotenciaRepository.Setup(r => r.ObterAsync(key))
                .ReturnsAsync((Idempotencia?)null);

            // vamos simular que a conta existe para avançar
            _mockContaRepository.Setup(r => r.ObterPorIdAsync("conta1"))
                .ReturnsAsync(new Conta { IdContaCorrente = "conta1", Ativo = true });

            var cmd = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = key,
                NumeroConta = null,
                TipoMovimento = 'C',
                Valor = -10
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                _service.EfetuarMovimentacaoAsync(cmd, actorContaId: "conta1", isSystem: true, CancellationToken.None));

            Assert.Contains("Valor deve ser positivo", ex.Message);
        }

        [Fact]
        public async Task EfetuarMovimentacaoAsync_WhenValid_ShouldCallRepositoriesOnce()
        {
            // Arrange
            var key = "k3";
            _mockIdempotenciaRepository.Setup(r => r.ObterAsync(key))
                .ReturnsAsync((Idempotencia?)null);

            var conta = new Conta { IdContaCorrente = "conta1", Ativo = true };
            _mockContaRepository.Setup(r => r.ObterPorIdAsync("conta1"))
                .ReturnsAsync(conta);

            var cmd = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = key,
                NumeroConta = null,
                TipoMovimento = 'D',
                Valor = 20
            };

            // Act
            await _service.EfetuarMovimentacaoAsync(cmd, actorContaId: "conta1", isSystem: true, CancellationToken.None);

            // Assert
            _mockMovimentoRepository.Verify(r => r.CriarAsync(It.Is<Movimento>(m =>
                m.IdContaCorrente == "conta1" &&
                m.Valor == 20 &&
                m.TipoMovimento == 'D'
            )), Times.Once);

            _mockIdempotenciaRepository.Verify(r => r.CriarAsync(It.Is<Idempotencia>(i =>
                i.ChaveIdempotencia == key &&
                i.Resultado == "SUCCESS"
            )), Times.Once);
        }
    }
}
