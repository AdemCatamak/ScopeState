using System.Threading;

namespace ScopeState
{
    public interface IScopeStateAccessor<TScopeState> where TScopeState : BaseScopeState
    {
        TScopeState ScopeState { get; set; }
    }

    public abstract class BaseScopeStateAccessor<TScopeState> : IScopeStateAccessor<TScopeState> where TScopeState : BaseScopeState
    {
        private static readonly AsyncLocal<ScopeStateHolder> _scopeStateHolder = new AsyncLocal<ScopeStateHolder>();

        public TScopeState ScopeState
        {
            get => _scopeStateHolder.Value?.ScopeState;
            set
            {
                ScopeStateHolder holder = _scopeStateHolder.Value;
                if (holder != null)
                {
                    // Clear current TraceInfo trapped in the AsyncLocals, as its done.
                    holder.ScopeState = default;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the TraceInfo in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _scopeStateHolder.Value = new ScopeStateHolder {ScopeState = value};
                }
            }
        }

        private class ScopeStateHolder
        {
            public TScopeState ScopeState;
        }
    }
}