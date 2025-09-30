using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Tests
{
    public abstract class IntegrationTestBase<TProgram, TFactory>
    : IClassFixture<TFactory>
    where TProgram : class
    where TFactory : WebApplicationFactory<TProgram>
    {
        protected readonly HttpClient _client;

        protected IntegrationTestBase(TFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
    }
}
