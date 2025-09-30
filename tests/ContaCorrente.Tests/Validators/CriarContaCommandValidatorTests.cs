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
    public class CriarContaCommandValidatorTests
    {
        private readonly CriarContaCommandValidator _validator = new();

        [Fact]
        public void Deve_Falhar_Quando_Nome_Vazio()
        {
            var command = new CriarContaCommand { Nome = "", Cpf = "12345678909", Senha = "123456" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Nome)
                  .WithErrorMessage("O nome é obrigatório.");
        }

        [Fact]
        public void Deve_Falhar_Quando_Nome_MuitoLongo()
        {
            var command = new CriarContaCommand { Nome = new string('A', 101), Cpf = "12345678909", Senha = "123456" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void Deve_Falhar_Quando_Cpf_Vazio()
        {
            var command = new CriarContaCommand { Nome = "Fulano", Cpf = "", Senha = "123456" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Cpf)
                  .WithErrorMessage("O CPF é obrigatório.");
        }

        [Fact]
        public void Deve_Falhar_Quando_Cpf_Invalido()
        {
            var command = new CriarContaCommand { Nome = "Fulano", Cpf = "00000000000", Senha = "123456" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Cpf)
                  .WithErrorMessage("CPF inválido.");
        }

        [Fact]
        public void Deve_Falhar_Quando_Senha_Vazia()
        {
            var command = new CriarContaCommand { Nome = "Fulano", Cpf = "12345678909", Senha = "" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Senha)
                  .WithErrorMessage("A senha é obrigatória.");
        }

        [Fact]
        public void Deve_Falhar_Quando_Senha_MuitoCurta()
        {
            var command = new CriarContaCommand { Nome = "Fulano", Cpf = "12345678909", Senha = "123" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Senha)
                  .WithErrorMessage("A senha deve ter pelo menos 6 caracteres.");
        }

        [Fact]
        public void Deve_Passar_Quando_Command_Valido()
        {
            var command = new CriarContaCommand { Nome = "Fulano", Cpf = "12345678909", Senha = "123456" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
