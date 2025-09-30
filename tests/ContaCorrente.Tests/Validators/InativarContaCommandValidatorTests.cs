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
    public class InativarContaCommandValidatorTests
    {
        private readonly InativarContaCommandValidator _validator = new();

        [Fact]
        public void Deve_Falhar_Quando_Senha_Vazia()
        {
            var command = new InativarContaCommand { Senha = "" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Senha)
                  .WithErrorMessage("A senha é obrigatória.");
        }

        [Fact]
        public void Deve_Passar_Quando_Senha_Preenchida()
        {
            var command = new InativarContaCommand { Senha = "minhasenha" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
