using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transferencia.Application.Commands;
using FluentValidation;

namespace Transferencia.Application.Validators
{
    public class EfetuarTransferenciaCommandValidator : AbstractValidator<EfetuarTransferenciaCommand>
    {
        public EfetuarTransferenciaCommandValidator()
        {
            RuleFor(x => x.IdentificacaoRequisicao)
            .NotEmpty().WithMessage("Identificação da requisição é obrigatória.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Identificação da requisição deve ser um GUID válido.");

            RuleFor(x => x.NumeroContaDestino)
                .GreaterThan(0).WithMessage("Número da conta de destino inválido.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor da transferência deve ser maior que zero.");
        }
    }
}
