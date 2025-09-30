using ContaCorrente.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Validators
{
    public class EfetuarMovimentacaoCommandValidator : AbstractValidator<EfetuarMovimentacaoCommand>
    {
        public EfetuarMovimentacaoCommandValidator()
        {
            RuleFor(x => x.IdempotencyKey)
                .NotEmpty().WithMessage("A identificação da requisição é obrigatória.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor da movimentação deve ser positivo.");

            RuleFor(x => x.TipoMovimento)
                .InclusiveBetween('C', 'D')
                .WithMessage("Tipo de movimento inválido. Só pode ser 'C' ou 'D'.");
        }
    }
}
