using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transferencia.Application.Commands;
using Transferencia.Application.Validators;
using FluentValidation.TestHelper;

namespace Transferencia.Tests.Validators
{
    public class EfetuarTransferenciaCommandValidatorTests
    {
        private readonly EfetuarTransferenciaCommandValidator _validator = new();

        [Fact]
        public void Deve_Falhar_Quando_IdentificacaoRequisicao_Vazia()
        {
            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = "",
                NumeroContaDestino = 123,
                Valor = 100
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.IdentificacaoRequisicao)
                  .WithErrorMessage("Identificação da requisição é obrigatória.");
        }

        [Fact]
        public void Deve_Falhar_Quando_IdentificacaoRequisicao_NaoForGuid()
        {
            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = "not-a-guid",
                NumeroContaDestino = 123,
                Valor = 100
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.IdentificacaoRequisicao)
                  .WithErrorMessage("Identificação da requisição deve ser um GUID válido.");
        }

        [Fact]
        public void Deve_Falhar_Quando_NumeroContaDestino_Invalido()
        {
            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = Guid.NewGuid().ToString(),
                NumeroContaDestino = 0,
                Valor = 100
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.NumeroContaDestino)
                  .WithErrorMessage("Número da conta de destino inválido.");
        }

        [Fact]
        public void Deve_Falhar_Quando_Valor_Invalido()
        {
            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = Guid.NewGuid().ToString(),
                NumeroContaDestino = 123,
                Valor = 0
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Valor)
                  .WithErrorMessage("O valor da transferência deve ser maior que zero.");
        }

        [Fact]
        public void Deve_Passar_Quando_Command_Valido()
        {
            var command = new EfetuarTransferenciaCommand
            {
                IdentificacaoRequisicao = Guid.NewGuid().ToString(),
                NumeroContaDestino = 123,
                Valor = 100
            };

            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
