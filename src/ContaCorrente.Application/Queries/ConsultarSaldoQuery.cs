using ContaCorrente.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Queries
{
    public class ConsultarSaldoQuery : IRequest<SaldoResponse>
    {
        public string IdContaCorrente { get; set; } = string.Empty; // Vem do token
    }
}
