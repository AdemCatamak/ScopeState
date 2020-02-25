using System;
using System.Threading.Tasks;
using MassTransit;

namespace ScopeState.MassTransitMiddleware
{
    public class ScopeStatePublishObserver<TScopeState> : IPublishObserver
        where TScopeState : BaseScopeState, new()
    {
        private readonly IScopeStateAccessor<TScopeState> _scopeStateAccessor;
        private readonly Action<PublishContext, TScopeState> _prePublish;

        public ScopeStatePublishObserver(IScopeStateAccessor<TScopeState> scopeStateAccessor, Action<PublishContext, TScopeState> prePublish = null)
        {
            _scopeStateAccessor = scopeStateAccessor;
            _prePublish = prePublish;
        }

        public Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            if (_prePublish == null)
            {
                if (context.Headers != null && !context.Headers.TryGetHeader("x-trace-id", out object _))
                {
                    string traceId = _scopeStateAccessor.ScopeState?.TraceId ?? new TScopeState().TraceId;
                    context.Headers?.Set("x-trace-id", traceId);
                }

                return Task.CompletedTask;
            }

            _prePublish(context, _scopeStateAccessor.ScopeState);
            return Task.CompletedTask;
        }

        public Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            return Task.CompletedTask;
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            return Task.CompletedTask;
        }
    }
}