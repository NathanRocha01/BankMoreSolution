using ContaCorrente.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Validators
{
    public class InativarContaCommandValidator : AbstractValidator<InativarContaCommand>
    {
        public InativarContaCommandValidator()
        {
            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória.");
        }
    }
}
