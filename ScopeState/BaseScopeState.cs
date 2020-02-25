using System;

namespace ScopeState
{
    public abstract class BaseScopeState
    {
        protected BaseScopeState()
        {
            TraceId = $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}--{Guid.NewGuid()}";
        }

        public string TraceId { get; set; }
    }
}