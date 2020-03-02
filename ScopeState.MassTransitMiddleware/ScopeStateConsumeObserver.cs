using System;
using System.Threading.Tasks;
using MassTransit;

namespace ScopeState.MassTransitMiddleware
{
    public class ScopeStateConsumeObserver<TScopeState> : IConsumeObserver where TScopeState : BaseScopeState, new()
    {
        private readonly IScopeStateAccessor<TScopeState> _scopeStateAccessor;
        private readonly Func<ConsumeContext, TScopeState> _generateScopeState;

        public ScopeStateConsumeObserver(IScopeStateAccessor<TScopeState> scopeStateAccessor, Func<ConsumeContext, TScopeState> generateScopeState = null)
        {
            _scopeStateAccessor = scopeStateAccessor;
            _generateScopeState = generateScopeState;
        }

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            var scopeState = new TScopeState
                             {
                                 TraceId = $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}--{context.MessageId?.ToString() ?? Guid.NewGuid().ToString()}"
                             };
            if (_generateScopeState == null)
            {
                if (context.Headers != null && context.Headers.TryGetHeader("x-trace-id", out object traceIdObj))
                {
                    var traceId = traceIdObj.ToString();
                    scopeState.TraceId = traceId;
                }
            }
            else
            {
                scopeState = _generateScopeState(context);
            }

            _scopeStateAccessor.ScopeState = scopeState;

            return Task.CompletedTask;
        }

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            return Task.CompletedTask;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            return Task.CompletedTask;
        }
    }
}