using Tarifa.Worker.Domain.Services;
using KafkaFlow;

namespace Tarifa.Worker.Consumers
{
    public class TransferenciaConsumer : IMessageHandler<TransferenciaRealizadaMessage>
    {
        private readonly ITarifaService _service;

        public TransferenciaConsumer(ITarifaService service)
        {
            _service = service;
        }

        public async Task Handle(IMessageContext context, TransferenciaRealizadaMessage message)
        {
            await _service.ProcessarTarifa(message.IdContaCorrenteOrigem);
        }
    }
    public record TransferenciaRealizadaMessage
    {
        public string IdContaCorrenteOrigem { get; init; }
    }
}
