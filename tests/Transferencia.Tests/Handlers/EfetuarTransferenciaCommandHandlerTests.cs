using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Shared.Entities;
using Shared.Exceptions;
using Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Transferencia.Application.Commands;
using Transferencia.Application.Handlers;
using Transferencia.Application.Producer;
using Transferencia.Domain.Entities;
using Transferencia.Domain.Interfaces;

namespace Transferencia.Tests.Handlers
{
    public class EfetuarTransferenciaCommandHandlerTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
        private readonly Mock<IDistributedCache> _cacheMock = new();
        private readonly Mock<ITransferenciaRepository> _transferenciaRepositoryMock = new();
        private readonly Mock<IIdempotenciaRepository> _idempotenciaRepositoryMock = new();
        private readonly Mock<ITransferenciaLogRepository> _logRepositoryMock = new();
        private readonly Mock<ITransferenciaProducer> _producer = new();
        private readonly EfetuarTransferenciaCommandHandler _handler;

        public EfetuarTransferenciaCommandHandlerTests()
        {
            _handler = new EfetuarTransferenciaCommandHandler(
                _httpContextAccessorMock.Object,
                _cacheMock.Object,
                _transferenciaRepositoryMock.Object,
                _idempotenciaRepositoryMock.Object,
                _logRepositoryMock.Object,
                _producer.Object
            );
        }

        private void SetUserContext(string idConta)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, idConta)
        }));
            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext { User = user });
        }

        [Fact]
        public async Task Handle_DeveRealizarTransferencia_QuandoDadosValidos()
        {
            // Arrange
            var idConta = Guid.NewGuid().ToString();
            SetUserContext(idConta);

            _cacheMock.Setup(x => x.GetAsync($"token:{idConta}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes("fake-jwt"));

            _transferenciaRepositoryMock.Setup(x => x.DebitarAsync("fake-jwt", 100)).ReturnsAsync(true);
            _transferenciaRepositoryMock.Setup(x => x.CreditarAsync("fake-jwt", 123, 100)).ReturnsAsync(true);

            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = Guid.NewGuid().ToString(),
                NumeroContaDestino = 123,
                Valor = 100
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _logRepositoryMock.Verify(x => x.CriarAsync(It.IsAny<TransferenciaEntity>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoTokenInvalido()
        {
            var idConta = Guid.NewGuid().ToString();
            SetUserContext(idConta);

            _cacheMock.Setup(x => x.GetAsync($"token:{idConta}", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = Guid.NewGuid().ToString(),
                NumeroContaDestino = 123,
                Valor = 100
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "INVALID_TOKEN");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoValorInvalido()
        {
            var idConta = Guid.NewGuid().ToString();
            SetUserContext(idConta);

            _cacheMock.Setup(x => x.GetAsync($"token:{idConta}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes("fake-jwt"));

            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = Guid.NewGuid().ToString(),
                NumeroContaDestino = 123,
                Valor = 0
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "INVALID_VALUE");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoFalhaNoDebito()
        {
            var idConta = Guid.NewGuid().ToString();
            SetUserContext(idConta);

            _cacheMock.Setup(x => x.GetAsync($"token:{idConta}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes("fake-jwt"));

            _transferenciaRepositoryMock.Setup(x => x.DebitarAsync("fake-jwt", 100)).ReturnsAsync(false);

            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = Guid.NewGuid().ToString(),
                NumeroContaDestino = 123,
                Valor = 100
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "INVALID_ACCOUNT");
        }

        [Fact]
        public async Task Handle_DeveEstornar_QuandoFalhaNoCredito()
        {
            var idConta = Guid.NewGuid().ToString();
            SetUserContext(idConta);

            _cacheMock.Setup(x => x.GetAsync($"token:{idConta}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes("fake-jwt"));

            _transferenciaRepositoryMock.Setup(x => x.DebitarAsync("fake-jwt", 100)).ReturnsAsync(true);
            _transferenciaRepositoryMock.Setup(x => x.CreditarAsync("fake-jwt", 123, 100)).ReturnsAsync(false);

            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = Guid.NewGuid().ToString(),
                NumeroContaDestino = 123,
                Valor = 100
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "TRANSFER_FAILED");

            _transferenciaRepositoryMock.Verify(x => x.EstornarAsync("fake-jwt", 100), Times.Once);
        }

        [Fact]
        public async Task Handle_DeveRetornarUnit_QuandoIdempotenciaSemResultado()
        {
            var idConta = Guid.NewGuid().ToString();
            SetUserContext(idConta);

            _idempotenciaRepositoryMock.Setup(x => x.ObterAsync(It.IsAny<string>()))
                .ReturnsAsync(new Idempotencia { Resultado = null });

            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = Guid.NewGuid().ToString(),
                NumeroContaDestino = 123,
                Valor = 100
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoIdempotenciaJaProcessada()
        {
            _idempotenciaRepositoryMock.Setup(x => x.ObterAsync(It.IsAny<string>()))
                .ReturnsAsync(new Idempotencia { Resultado = "SUCCESS" });

            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = Guid.NewGuid().ToString(),
                NumeroContaDestino = 123,
                Valor = 100
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "DUPLICATE_REQUEST");
        }
    }
}
