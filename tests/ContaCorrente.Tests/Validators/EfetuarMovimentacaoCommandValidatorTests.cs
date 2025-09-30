using ContaCorrente.Application.Commands;
using ContaCorrente.Application.Validators;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Tests.Validators
{
    public class EfetuarMovimentacaoCommandValidatorTests
    {
        private readonly EfetuarMovimentacaoCommandValidator _validator = new();

        [Fact]
        public void Deve_Falhar_Quando_IdempotencyKey_Vazio()
        {
            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = "",
                Valor = 100,
                TipoMovimento = 'C'
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.IdempotencyKey)
                  .WithErrorMessage("A identificação da requisição é obrigatória.");
        }

        [Fact]
        public void Deve_Falhar_Quando_Valor_NaoPositivo()
        {
            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = 0,
                TipoMovimento = 'C'
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Valor)
                  .WithErrorMessage("O valor da movimentação deve ser positivo.");
        }

        [Theory]
        [InlineData('X')]
        [InlineData(' ')]
        [InlineData('1')]
        public void Deve_Falhar_Quando_TipoMovimento_Invalido(char tipo)
        {
            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = 100,
                TipoMovimento = tipo
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.TipoMovimento)
                  .WithErrorMessage("Tipo de movimento inválido. Só pode ser 'C' ou 'D'.");
        }

        [Theory]
        [InlineData('C')]
        [InlineData('D')]
        public void Deve_Passar_Quando_Command_Valido(char tipo)
        {
            var command = new EfetuarMovimentacaoCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = 100,
                TipoMovimento = tipo
            };

            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
