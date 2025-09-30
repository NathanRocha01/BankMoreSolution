using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Transferencia.Domain.Interfaces;

namespace Transferencia.Infrastructure.Repositories
{
    public class TransferenciaRepository : ITransferenciaRepository
    {
        private readonly HttpClient _httpClient;

        public TransferenciaRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> DebitarAsync(string token, decimal valor)
        {
            var payload = new
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = valor,
                TipoMovimento = "D"
            };

            return await EnviarMovimentoAsync(token, payload);
        }

        public async Task<bool> CreditarAsync(string token, int numeroContaDestino, decimal valor)
        {
            var payload = new
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                NumeroConta = numeroContaDestino,
                Valor = valor,
                TipoMovimento = "C"
            };

            return await EnviarMovimentoAsync(token, payload);
        }

        public async Task<bool> EstornarAsync(string token, decimal valor)
        {
            var payload = new
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                Valor = valor,
                TipoMovimento = "C"
            };

            return await EnviarMovimentoAsync(token, payload);
        }

        public async Task<bool> EnviarMovimentoAsync(string token, object payload)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/Movimento");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
