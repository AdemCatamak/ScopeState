using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ScopeState;
using ScopeState.HttpClientInterceptor;
using ScopeState.Imp;

namespace IntegrationTest.ScopeState.HttpClientInterceptor
{
    public class Tests
    {
        private class TestHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK), cancellationToken);
            }
        }

        private Mock<IScopeStateAccessor<BasicScopeState>> _mockScopeStateAccessor;
        private ScopeStateHttpClientDelegatingHandler<BasicScopeState> _sut;

        [SetUp]
        public void SetUp()
        {
            _mockScopeStateAccessor = new Mock<IScopeStateAccessor<BasicScopeState>>();
            _sut = new ScopeStateHttpClientDelegatingHandler<BasicScopeState>(_mockScopeStateAccessor.Object)
                   {
                       InnerHandler = new TestHandler()
                   };
        }

        [TearDown]
        public void TearDown()
        {
            _sut?.Dispose();
        }

        [Test]
        public async Task WhenScopeStateDelegatingHandlerDefaultIsUsed__TraceIdIsSetIntoRequestHeaderAutomatically()
        {
            const string traceId = nameof(WhenScopeStateDelegatingHandlerDefaultIsUsed__TraceIdIsSetIntoRequestHeaderAutomatically);
            _mockScopeStateAccessor.Setup(accessor => accessor.ScopeState)
                                   .Returns(new BasicScopeState()
                                            {
                                                TraceId = traceId
                                            });

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            using var httpClient = new HttpClient(_sut);
            using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            Assert.NotNull(httpResponseMessage);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.True(httpRequestMessage.Headers.TryGetValues("x-trace-id", out IEnumerable<string> traceIdValues));
            Assert.Contains(traceId, traceIdValues.ToList());
        }

        [Test]
        public async Task WhenScopeStateDelegatingHandlerDefaultIsUsed_IfRequestHasHeader_TraceIdIsNotSetIntoRequestHeader()
        {
            const string traceId = nameof(WhenScopeStateDelegatingHandlerDefaultIsUsed_IfRequestHasHeader_TraceIdIsNotSetIntoRequestHeader);
            const string expectedTraceId = "some-trace-id";
            _mockScopeStateAccessor.Setup(accessor => accessor.ScopeState)
                                   .Returns(new BasicScopeState()
                                            {
                                                TraceId = traceId
                                            });

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            httpRequestMessage.Headers.Add("x-trace-id", new[] {expectedTraceId});
            using var httpClient = new HttpClient(_sut);
            using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            Assert.NotNull(httpResponseMessage);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.IsTrue(httpRequestMessage.Headers.TryGetValues("x-trace-id", out IEnumerable<string> traceIdValues));
            List<string> traceIdValueList = traceIdValues.ToList();
            Assert.Contains(expectedTraceId, traceIdValueList.ToList());
            Assert.IsTrue(traceIdValueList.All(x => x != traceId));
        }
    }
}