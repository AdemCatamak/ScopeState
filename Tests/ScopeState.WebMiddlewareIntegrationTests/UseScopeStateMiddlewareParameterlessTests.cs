using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ScopeState.NetCoreDIExtensions;
using ScopeState.WebMiddleware;
using Xunit;

namespace ScopeState.WebMiddlewareIntegrationTests
{
    public class DefaultScopeStateMiddlewareTestServer : IDisposable
    {
        public TestServer TestServer { get; }

        public DefaultScopeStateMiddlewareTestServer()
        {
            TestServer = new TestServer(new WebHostBuilder()
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

        public void Dispose()
        {
            TestServer?.Dispose();
        }
    }

    public class UseScopeStateMiddlewareParameterlessTests : IClassFixture<DefaultScopeStateMiddlewareTestServer>
    {
        private readonly TestServer _server;

        public UseScopeStateMiddlewareParameterlessTests(DefaultScopeStateMiddlewareTestServer defaultScopeStateMiddlewareTestServer)
        {
            _server = defaultScopeStateMiddlewareTestServer.TestServer;
        }

        [Fact]
        public async Task WhenRequestDoesNotContainsHeaders__ScopeStateGenerateOwnTraceId()
        {
            using HttpClient httpClient = _server.CreateClient();
            using HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("default-scope-state-accessor/trace-id");
            string content = await httpResponseMessage.Content.ReadAsStringAsync();

            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task WhenRequestContainsOwnTraceId__ScopeStateUseGivenTraceId()
        {
            using HttpClient httpClient = _server.CreateClient();
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "default-scope-state-accessor/trace-id");
            httpRequestMessage.Headers.Add("x-trace-id", "42");

            using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            string content = await httpResponseMessage.Content.ReadAsStringAsync();

            Assert.NotEmpty(content);
            Assert.Equal("42", content);
        }
    }
}