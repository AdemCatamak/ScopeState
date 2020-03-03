using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ScopeState.HttpClientInterceptor
{
    public class ScopeStateHttpClientDelegatingHandler<TScopeState> : DelegatingHandler
        where TScopeState : BaseScopeState
    {
        private readonly IScopeStateAccessor<TScopeState> _scopeStateAccessor;
        private readonly Action<HttpRequestMessage, TScopeState> _preSend;

        public ScopeStateHttpClientDelegatingHandler(IScopeStateAccessor<TScopeState> scopeStateAccessor, Action<HttpRequestMessage, TScopeState> preSend = null)
        {
            _scopeStateAccessor = scopeStateAccessor;
            _preSend = preSend ?? PreSend;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _preSend(request, _scopeStateAccessor.ScopeState);

            HttpResponseMessage httpResponseMessage = await base.SendAsync(request, cancellationToken);

            return httpResponseMessage;
        }

        private static void PreSend(HttpRequestMessage requestMessage, TScopeState scopeState)
        {
            const string traceIdHeader = "x-trace-id";
            if (requestMessage.Headers.All(h => h.Key != traceIdHeader))
            {
                requestMessage.Headers.Add(traceIdHeader, scopeState?.TraceId);
            }
        }
    }
}