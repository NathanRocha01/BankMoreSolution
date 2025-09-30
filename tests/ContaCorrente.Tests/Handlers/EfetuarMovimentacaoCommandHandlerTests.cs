using ContaCorrente.Application.Commands;
using ContaCorrente.Application.Handlers;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interface;
using ContaCorrente.Tests.Fixtures;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
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

namespace ContaCorrente.Tests.Handlers
{
    public class EfetuarMovimentacaoCommandHandlerTests
    {
        private readonly Mock<IContaRepository> _contaRepositoryMock = new();
        private readonly Mock<IMovimentoRepository> _movimentoRepositoryMock = new();
        private readonly Mock<IIdempotenciaRepository> _idempotenciaRepositoryMock = new();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
        private readonly EfetuarMovimentacaoCommandHandler _handler;

        public EfetuarMovimentacaoCommandHandlerTests()
        {
            _handler = new EfetuarMovimentacaoCommandHandler(
                _httpContextAccessorMock.Object,
                _contaRepositoryMock.Object,
                _movimentoRepositoryMock.Object,
                _idempotenciaRepositoryMock.Object
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
        public async Task Handle_DeveCriarMovimento_QuandoDadosValidos()
        {
            // Arrange
            var conta = ContaFixture.CriarContaValida();
            SetUserContext(conta.IdContaCorrente.ToString());

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(conta.IdContaCorrente.ToString()))
                .ReturnsAsync(conta);

            _idempotenciaRepositoryMock.Setup(x => x.ObterAsync(It.IsAny<string>()))
                .ReturnsAsync((Idempotencia?)null);

            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = 100,
                TipoMovimento = 'C'
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _movimentoRepositoryMock.Verify(x => x.CriarAsync(It.IsAny<Movimento>()), Times.Once);
            _idempotenciaRepositoryMock.Verify(x => x.CriarAsync(It.IsAny<Idempotencia>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoTokenInvalido()
        {
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = 100,
                TipoMovimento = 'C'
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "INVALID_TOKEN");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoContaNaoEncontrada()
        {
            SetUserContext(Guid.NewGuid().ToString());
            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Conta?)null);

            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = 100,
                TipoMovimento = 'C'
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "INVALID_ACCOUNT");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoContaInativa()
        {
            var conta = ContaFixture.CriarContaValida();
            conta.Ativo = false;
            SetUserContext(conta.IdContaCorrente.ToString());

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(conta.IdContaCorrente.ToString()))
                .ReturnsAsync(conta);

            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = 100,
                TipoMovimento = 'C'
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "INACTIVE_ACCOUNT");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoValorInvalido()
        {
            var conta = ContaFixture.CriarContaValida();
            SetUserContext(conta.IdContaCorrente.ToString());

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(conta.IdContaCorrente.ToString()))
                .ReturnsAsync(conta);

            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = -10,
                TipoMovimento = 'C'
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "INVALID_VALUE");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoTipoInvalido()
        {
            var conta = ContaFixture.CriarContaValida();
            SetUserContext(conta.IdContaCorrente.ToString());

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(conta.IdContaCorrente.ToString()))
                .ReturnsAsync(conta);

            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = 100,
                TipoMovimento = 'X'
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "INVALID_TYPE");
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoDebitoEmContaDeTerceiro()
        {
            var conta = ContaFixture.CriarContaValida();
            SetUserContext(Guid.NewGuid().ToString()); // outro usuário

            _contaRepositoryMock.Setup(x => x.ObterPorNumeroAsync(conta.Numero))
                .ReturnsAsync(conta);

            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                NumeroConta = conta.Numero,
                Valor = 100,
                TipoMovimento = 'D'
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "INVALID_TYPE");
        }

        [Fact]
        public async Task Handle_DeveRetornarUnit_QuandoIdempotenciaExistenteSemResultado()
        {
            var conta = ContaFixture.CriarContaValida();
            SetUserContext(conta.IdContaCorrente.ToString());

            _contaRepositoryMock.Setup(x => x.ObterPorIdAsync(conta.IdContaCorrente.ToString()))
                .ReturnsAsync(conta);

            _idempotenciaRepositoryMock.Setup(x => x.ObterAsync(It.IsAny<string>()))
                .ReturnsAsync(new Idempotencia { Resultado = null });

            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = 100,
                TipoMovimento = 'C'
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);
            _movimentoRepositoryMock.Verify(x => x.CriarAsync(It.IsAny<Movimento>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DeveLancarExcecao_QuandoIdempotenciaJaProcessada()
        {
            var conta = ContaFixture.CriarContaValida();
            SetUserContext(conta.IdContaCorrente.ToString());

            _idempotenciaRepositoryMock.Setup(x => x.ObterAsync(It.IsAny<string>()))
                .ReturnsAsync(new Idempotencia { Resultado = "SUCCESS" });

            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = 100,
                TipoMovimento = 'C'
            };

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should()
                .ThrowAsync<DomainException>()
                .Where(e => e.ErrorCode == "DUPLICATE_REQUEST");
        }
    }
}
