using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarifa.Worker.Domain.Entities;
using Tarifa.Worker.Infrastructure;
using Tarifa.Worker.Producers;

namespace Tarifa.Worker.Domain.Services
{
    public interface ITarifaService
    {
        Task ProcessarTarifa(string idContaCorrente);
    }

    public class TarifaService : ITarifaService
    {
        private readonly ITarifaRepository _repo;
        private readonly TarifaProducer _producer;
        private readonly decimal _valorTarifa;

        public TarifaService(ITarifaRepository repo, TarifaProducer producer, IConfiguration config)
        {
            _repo = repo;
            _producer = producer;
            _valorTarifa = config.GetSection("TarifaConfig").GetValue<decimal>("ValorTarifa");
        }

        public async Task ProcessarTarifa(string idContaCorrente)
        {
            var tarifa = new TarifaE
            {
                IdContaCorrente = idContaCorrente,
                Valor = _valorTarifa
            };

            await _repo.AddAsync(tarifa);
            await _producer.PublicarTarifacao(tarifa);
        }
    }
}
