using ContaCorrente.Application.Commands;
using ContaCorrente.Application.Service;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interface;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Entities;
using Shared.Exceptions;
using Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContaCorrente.Application.Handlers
{
    public class EfetuarMovimentacaoCommandHandler : IRequestHandler<EfetuarMovimentacaoCommand, Unit>
    {
        private readonly IHttpContextAccessor _http;
        private readonly IContaCorrenteAppService _service;

        public EfetuarMovimentacaoCommandHandler(IHttpContextAccessor http, IContaCorrenteAppService service)
        {
            _http = http;
            _service = service;
        }

        public async Task<Unit> Handle(EfetuarMovimentacaoCommand request, CancellationToken ct)
        {
            var actorId = _http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _service.EfetuarMovimentacaoAsync(request, actorId, isSystem: false, ct);
            return Unit.Value;
        }
    }
}
