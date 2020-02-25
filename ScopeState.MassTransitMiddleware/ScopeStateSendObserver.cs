using System;
using System.Threading.Tasks;
using MassTransit;

namespace ScopeState.MassTransitMiddleware
{
    public class ScopeStateSendObserver<TScopeState> : ISendObserver
        where TScopeState : BaseScopeState
    {
        private readonly IScopeStateAccessor<TScopeState> _scopeStateAccessor;
        private readonly Action<SendContext, TScopeState> _preSend;

        public ScopeStateSendObserver(IScopeStateAccessor<TScopeState> scopeStateAccessor, Action<SendContext, TScopeState> preSend = null)
        {
            _scopeStateAccessor = scopeStateAccessor;
            _preSend = preSend;
        }

        public Task PreSend<T>(SendContext<T> context) where T : class
        {
            if (_preSend == null)
            {
                if (!context.Headers.TryGetHeader("x-trace-id", out object _))
                {
                    string traceId = _scopeStateAccessor.ScopeState.TraceId;
                    context.Headers.Set("x-trace-id", traceId);
                }

                return Task.CompletedTask;
            }

            _preSend(context, _scopeStateAccessor.ScopeState);
            return Task.CompletedTask;
        }

        public Task PostSend<T>(SendContext<T> context) where T : class
        {
            return Task.CompletedTask;
        }

        public Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
        {
            return Task.CompletedTask;
        }
    }
}