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
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator = new();

        [Fact]
        public void Deve_Falhar_Quando_Senha_Vazia()
        {
            var command = new LoginCommand { Cpf = "12345678909", NumeroConta = null, Senha = "" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Senha)
                  .WithErrorMessage("Senha é obrigatória.");
        }

        [Fact]
        public void Deve_Falhar_Quando_NaoInformarCpfNemNumeroConta()
        {
            var command = new LoginCommand { Cpf = "", NumeroConta = null, Senha = "123456" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("CPF ou número da conta devem ser informados.");
        }

        [Fact]
        public void Deve_Passar_Quando_InformarCpfValido()
        {
            var command = new LoginCommand { Cpf = "49631509052", NumeroConta = null, Senha = "123456" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Deve_Passar_Quando_InformarNumeroContaValido()
        {
            var command = new LoginCommand { Cpf = null, NumeroConta = 1234, Senha = "123456" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
