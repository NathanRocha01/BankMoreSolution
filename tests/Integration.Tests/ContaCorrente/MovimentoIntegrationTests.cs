using FluentAssertions;
using Integration.Tests.Factorys;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Integration.Tests.ContaCorrente
{
    public class MovimentoIntegrationTests
        : IntegrationTestBase<ProgramC, ContaWebApplicationFactory<ProgramC>>
    {
        public MovimentoIntegrationTests(ContaWebApplicationFactory<ProgramC> factory)
            : base(factory) { }

        private async Task<string> CriarContaELogar(string nome, string cpf, string senha)
        {
            // Criar conta
            var criarConta = new { Nome = nome, Cpf = cpf, Senha = senha };
            var respCriar = await _client.PostAsJsonAsync("/api/conta", criarConta);
            respCriar.EnsureSuccessStatusCode();

            // Login
            var loginBody = new { Cpf = cpf, Senha = senha };
            var respLogin = await _client.PostAsJsonAsync("/api/login", loginBody);
            respLogin.EnsureSuccessStatusCode();

            var loginObj = await respLogin.Content.ReadFromJsonAsync<JsonElement>();
            return loginObj.GetProperty("token").GetString()!;
        }

        [Fact]
        public async Task Deve_EfetuarCredito_QuandoDadosValidos()
        {
            var token = await CriarContaELogar("Fulano Movimento", CpfGenerator.GerarCpfValido(), "senha123");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var movimento = new
            {
                NumeroConta = (int?)null, // movimenta a própria conta
                Valor = 100.00m,
                TipoMovimento = 'C',
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var resp = await _client.PostAsJsonAsync("/api/movimento", movimento);

            resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Deve_Falhar_QuandoValorNegativo()
        {
            var token = await CriarContaELogar("Ciclano Movimento", CpfGenerator.GerarCpfValido(), "senha123");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var movimento = new
            {
                NumeroConta = (int?)null,
                Valor = -10.00m,
                TipoMovimento = 'D',
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var resp = await _client.PostAsJsonAsync("/api/movimento", movimento);

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
            string type = result.GetProperty("type").GetString()!;

            type.Should().Be("INVALID_VALUE");
        }

        [Fact]
        public async Task Deve_Falhar_QuandoTipoMovimentoInvalido()
        {
            var token = await CriarContaELogar("Beltrano Movimento", CpfGenerator.GerarCpfValido(), "senha123");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var movimento = new
            {
                NumeroConta = (int?)null,
                Valor = 50.00m,
                TipoMovimento = 'X', // inválido
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var resp = await _client.PostAsJsonAsync("/api/movimento", movimento);

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
            string type = result.GetProperty("type").GetString()!;

            type.Should().Be("INVALID_TYPE");
        }

        [Fact]
        public async Task Deve_FazerCreditoEmContaTerceiro()
        {
            // Criar duas contas
            var tokenOrigem = await CriarContaELogar("Origem", CpfGenerator.GerarCpfValido(), "senha123");

            var criarContaDestino = new { Nome = "Destino", Cpf = CpfGenerator.GerarCpfValido(), Senha = "senha456" };
            var respCriarDestino = await _client.PostAsJsonAsync("/api/conta", criarContaDestino);
            respCriarDestino.EnsureSuccessStatusCode();

            var contaDestino = await respCriarDestino.Content.ReadFromJsonAsync<JsonElement>();
            int numeroContaDestino = contaDestino.GetProperty("numeroConta").GetInt32();

            // Autenticar origem
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenOrigem);

            var movimento = new
            {
                NumeroConta = numeroContaDestino,
                Valor = 75.00m,
                TipoMovimento = 'C',
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var resp = await _client.PostAsJsonAsync("/api/movimento", movimento);

            resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Deve_Falhar_DebitoEmContaTerceiro()
        {
            var tokenOrigem = await CriarContaELogar("Origem Debito", CpfGenerator.GerarCpfValido(), "senha123");

            var criarContaDestino = new { Nome = "Vitima", Cpf = CpfGenerator.GerarCpfValido(), Senha = "senha456" };
            var respCriarDestino = await _client.PostAsJsonAsync("/api/conta", criarContaDestino);
            respCriarDestino.EnsureSuccessStatusCode();

            var contaDestino = await respCriarDestino.Content.ReadFromJsonAsync<JsonElement>();
            int numeroContaDestino = contaDestino.GetProperty("numeroConta").GetInt32();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenOrigem);

            var movimento = new
            {
                NumeroConta = numeroContaDestino,
                Valor = 50.00m,
                TipoMovimento = 'D',
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var resp = await _client.PostAsJsonAsync("/api/movimento", movimento);

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
            string type = result.GetProperty("type").GetString()!;

            type.Should().Be("INVALID_TYPE");
        }
    }
}
