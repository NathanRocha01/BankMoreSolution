using ContaCorrente.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Validators
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("Senha é obrigatória.");

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Cpf) || x.NumeroConta.HasValue)
                .WithMessage("CPF ou número da conta devem ser informados.");
        }
    }
}
