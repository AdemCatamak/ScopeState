using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScopeState.NetCoreDIExtensions;
using ScopeState.WebMiddleware;
using Xunit;

namespace ScopeState.WebMiddlewareIntegrationTests
{
    public class UseScopeStateMiddlewareParameterlessTests : IDisposable
    {
        private readonly TestServer _server;

        public UseScopeStateMiddlewareParameterlessTests()
        {
            _server = new TestServer(new WebHostBuilder()
                                    .ConfigureServices(services =>
                                                       {
                                                           services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
                                                           services.AddScopeStateAccessor();
                                                       })
                                    .Configure(builder =>
                                               {
                                                   builder.UseScopeStateMiddleware();
                                                   builder.UseMvc();
                                               })
                                    );
        }

        [Fact]
        public async Task WhenRequestDoesNotContainsHeaders__ScopeStateGenerateOwnTraceId()
        {
            HttpClient httpClient = _server.CreateClient();
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("default-scope-state-accessor/trace-id");
            string content = await httpResponseMessage.Content.ReadAsStringAsync();

            Assert.NotEmpty(content);
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}