using KafkaFlow.Producers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Messages;

namespace Transferencia.Application.Producer
{
    public class TransferenciaProducer : ITransferenciaProducer
    {
        private readonly IProducerAccessor _producerAccessor;
        private readonly string _topic;

        public TransferenciaProducer(IProducerAccessor producerAccessor, IConfiguration config)
        {
            _producerAccessor = producerAccessor;
            _topic = config["Kafka:TopicTransferencias"];
        }

        public Task PublicarAsync(TransferenciaRealizadaMessage msg)
        {
            var producer = _producerAccessor.GetProducer<TransferenciaRealizadaMessage>();
            Console.WriteLine($"[Producer] Enviando mensagem para tópico {_topic}: {JsonSerializer.Serialize(msg)}");
            return producer.ProduceAsync(_topic, msg)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine($"[Producer] Falha ao enviar mensagem: {t.Exception?.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"[Producer] Mensagem enviada com sucesso para {_topic}");
                    }
                });
        }
    }
}
