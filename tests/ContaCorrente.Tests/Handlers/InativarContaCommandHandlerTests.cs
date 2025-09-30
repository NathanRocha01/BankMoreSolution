using ContaCorrente.Application.Commands;
using ContaCorrente.Application.Handlers;
using ContaCorrente.Domain.Interface;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ContaCorrente.Tests.Fixtures;
using ContaCorrente.Domain.Entities;

namespace ContaCorrente.Tests.Handlers
{
    public class InativarContaCommandHandlerTests
    {
        private readonly Mock<IContaRepository> _contaRepositoryMock = new();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
        private readonly InativarContaCommandHandler _handler;

        public InativarContaCommandHandlerTests()
        {
            _handler = new InativarContaCommandHandler(
                _contaRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task Handle_DeveInativarConta_QuandoSenhaValida()
        {
            // Arrange
            var conta = ContaFixture.CriarContaValida("senha123");

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, conta.IdContaCorrente.ToString())
        }));

            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext { User = user });

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(conta.IdContaCorrente.ToString()))
                .ReturnsAsync(conta);

            var command = new InativarContaCommand { Senha = "senha123" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            conta.Ativo.Should().BeFalse();
            _contaRepositoryMock.Verify(x => x.AtualizarAsync(conta), Times.Once);
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoTokenInvalido()
        {
            // Arrange
            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext { User = new ClaimsPrincipal() });

            var command = new InativarContaCommand { Senha = "qualquer" };

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Usuário não identificado.*")
                .Where(e => e.ErrorCode == "INVALID_TOKEN");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoContaNaoEncontrada()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }));

            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext { User = user });

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Conta?)null);

            var command = new InativarContaCommand { Senha = "senha123" };

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Conta não encontrada.*")
                .Where(e => e.ErrorCode == "INVALID_ACCOUNT");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoSenhaIncorreta()
        {
            // Arrange
            var conta = ContaFixture.CriarContaValida("senhaCorreta");

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, conta.IdContaCorrente.ToString())
        }));

            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext { User = user });

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(conta.IdContaCorrente.ToString()))
                .ReturnsAsync(conta);

            var command = new InativarContaCommand { Senha = "senhaErrada" };

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Senha inválida.*")
                .Where(e => e.ErrorCode == "USER_UNAUTHORIZED");
        }
    }
}
