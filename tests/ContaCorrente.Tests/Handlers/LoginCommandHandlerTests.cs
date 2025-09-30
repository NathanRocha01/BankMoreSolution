using ContaCorrente.Application.Commands;
using ContaCorrente.Application.Handlers;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interface;
using ContaCorrente.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ContaCorrente.Tests.Handlers
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IContaRepository> _contaRepositoryMock = new();
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
        private readonly Mock<IDistributedCache> _cacheMock = new();
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _handler = new LoginCommandHandler(
                _contaRepositoryMock.Object,
                _jwtTokenGeneratorMock.Object,
                _cacheMock.Object
            );
        }

        [Fact]
        public async Task Handle_DeveRetornarToken_QuandoLoginValidoPorNumeroConta()
        {
            // Arrange
            var salt = "salt123";
            var senha = "senha123";
            var senhaHash = HashSenha(senha, salt);

            var conta = new Conta
            {
                IdContaCorrente = Guid.NewGuid().ToString(),
                Nome = "Silva da Silva",
                Cpf = "49631509052",
                Numero = 123,
                Salt = salt,
                Senha = senhaHash
            };

            _contaRepositoryMock.Setup(x => x.ObterPorNumeroAsync(conta.Numero))
                .ReturnsAsync(conta);

            var expectedToken = "token.jwt.fake";
            _jwtTokenGeneratorMock.Setup(x => x.GerarToken(conta.IdContaCorrente))
                .Returns(expectedToken);

            var command = new LoginCommand { NumeroConta = conta.Numero, Senha = senha };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(expectedToken);
            _cacheMock.Verify(x =>
            x.SetAsync(
                $"token:{conta.IdContaCorrente}",
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == expectedToken),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoContaNaoEncontrada()
        {
            // Arrange
            _contaRepositoryMock.Setup(x => x.ObterPorCpfAsync("12345678900"))
                .ReturnsAsync((Conta?)null);

            var command = new LoginCommand { Cpf = "12345678900", Senha = "senha" };

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Usuário não encontrado.*")
                .Where(e => e.ErrorCode == "USER_UNAUTHORIZED");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoSenhaIncorreta()
        {
            var salt = "salt123";
            var senha = "senha123";
            var senhaHash = HashSenha(senha, salt);

            // Arrange
            var conta = new Conta
            {
                IdContaCorrente = Guid.NewGuid().ToString(),
                Nome = "Silva da Silva",
                Cpf = "49631509052",
                Numero = 123,
                Salt = salt,
                Senha = senhaHash
            };

            _contaRepositoryMock.Setup(x => x.ObterPorNumeroAsync(conta.Numero))
                .ReturnsAsync(conta);

            var command = new LoginCommand { NumeroConta = conta.Numero, Senha = "senhaErrada" };

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Senha inválida.*")
                .Where(e => e.ErrorCode == "USER_UNAUTHORIZED");
        }

        private string HashSenha(string senha, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(senha + salt);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
