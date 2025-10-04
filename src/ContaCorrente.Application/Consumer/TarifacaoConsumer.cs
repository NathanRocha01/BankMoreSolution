using ContaCorrente.Application.Service;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Messages;

namespace ContaCorrente.Application.Consumer
{
    public class TarifacaoConsumer : IMessageHandler<TarifaRealizadaMessage>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TarifacaoConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task Handle(IMessageContext context, TarifaRealizadaMessage message)
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IContaCorrenteAppService>();

            var idem = Guid.NewGuid().ToString();

            Console.WriteLine($"[Consumer] Mensagem recebida no tópico : {JsonSerializer.Serialize(message)}");

            await service.DebitarTarifaAsync(message.IdContaCorrente, message.Valor, idem, CancellationToken.None);
        }
    }
}
