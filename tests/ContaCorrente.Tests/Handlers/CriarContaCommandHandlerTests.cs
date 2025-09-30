using ContaCorrente.Application.Commands;
using ContaCorrente.Application.Handlers;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interface;
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
    public class CriarContaCommandHandlerTests
    {
        private readonly Mock<IContaRepository> _contaRepositoryMock = new();
        private readonly CriarContaCommandHandler _handler;

        public CriarContaCommandHandlerTests()
        {
            _handler = new CriarContaCommandHandler(_contaRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_DeveCriarConta_QuandoCpfValido()
        {
            // Arrange
            var command = new CriarContaCommand
            {
                Nome = "Usuário Teste",
                Cpf = "49631509052", // precisa passar num EhCpfValido()
                Senha = "senha123"
            };

            _contaRepositoryMock.Setup(x => x.VerificarExistenciaNumeroAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            // Act
            var numeroConta = await _handler.Handle(command, CancellationToken.None);

            // Assert
            numeroConta.Should().BeInRange(100000, 999999);
            _contaRepositoryMock.Verify(x => x.CriarAsync(It.IsAny<Conta>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoCpfInvalido()
        {
            // Arrange
            var command = new CriarContaCommand
            {
                Nome = "Teste",
                Cpf = "00000000000", // inválido
                Senha = "123"
            };

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("CPF inválido.*")
                .Where(e => e.ErrorCode == "INVALID_DOCUMENT");
        }

        [Fact]
        public async Task Handle_DeveGerarNovoNumero_QuandoNumeroContaJaExistir()
        {
            // Arrange
            var command = new CriarContaCommand
            {
                Nome = "Usuário",
                Cpf = "49631509052", // válido
                Senha = "senha"
            };

            // Primeira chamada -> true (já existe), segunda -> false (livre)
            _contaRepositoryMock.SetupSequence(x => x.VerificarExistenciaNumeroAsync(It.IsAny<int>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // Act
            var numeroConta = await _handler.Handle(command, CancellationToken.None);

            // Assert
            numeroConta.Should().BeInRange(100000, 999999);
            _contaRepositoryMock.Verify(x => x.CriarAsync(It.Is<Conta>(c => c.Numero == numeroConta)), Times.Once);
        }
    }
}
