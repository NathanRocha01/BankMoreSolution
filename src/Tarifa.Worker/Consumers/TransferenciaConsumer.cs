using Tarifa.Worker.Domain.Services;
using KafkaFlow;
using Shared.Messages;

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
            try
            {
                Console.WriteLine("[DEBUG] Entrou no handler");
                Console.WriteLine($"[DEBUG] Payload: {System.Text.Json.JsonSerializer.Serialize(message)}");
                await _service.ProcessarTarifa(message.IdContaCorrenteOrigem);
                Console.WriteLine("[DEBUG] Serviço executado com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] EXCEÇÃO no handler: {ex}");
                throw; 
            }
        }
    }
}
