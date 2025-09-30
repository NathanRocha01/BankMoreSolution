using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using Transferencia.Application.Commands;

namespace Transferencia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferenciaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransferenciaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EfetuarTransferencia([FromBody] EfetuarTransferenciaCommand command)
        {
            try
            {
                await _mediator.Send(command);
                return NoContent();
            }
            catch (DomainException ex)
            {
                return BadRequest(new { message = ex.Message, type = ex.ErrorCode });
            }
        }
    }
}
