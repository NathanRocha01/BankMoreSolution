using ContaCorrente.Application.Commands;
using ContaCorrente.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContaCorrente.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarContaCommand command)
        {
            var numeroConta = await _mediator.Send(command);
            return Ok(new { numeroConta });
        }

        [Authorize]
        [HttpPatch("inativar")]
        public async Task<IActionResult> Inativar([FromBody] InativarContaCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [Authorize]
        [HttpGet("saldo")]
        public async Task<IActionResult> Saldo()
        {
            var idConta = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new ConsultarSaldoQuery { IdContaCorrente = idConta });
            return Ok(result);
        }
    }
}
