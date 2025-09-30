using KafkaFlow;
using KafkaFlow.Producers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarifa.Worker.Domain.Entities;

namespace Tarifa.Worker.Producers
{
    public class TarifaProducer
    {
        private readonly IProducerAccessor _producerAccessor;
        private readonly string _topic;

        public TarifaProducer(IProducerAccessor producerAccessor, IConfiguration config)
        {
            _producerAccessor = producerAccessor;
            _topic = config["Kafka:TopicTarifas"];
        }

        public Task PublicarTarifacao(TarifaE tarifa)
        {
            var msg = new TarifaRealizadaMessage
            {
                IdContaCorrente = tarifa.IdContaCorrente,
                Valor = tarifa.Valor
            };

            var producer = _producerAccessor.GetProducer("tarifas-producer");

            return producer.ProduceAsync(_topic, msg);
        }
    }

    public record TarifaRealizadaMessage
    {
        public string IdContaCorrente { get; set; }
        public decimal Valor { get; set; }
    }
}
