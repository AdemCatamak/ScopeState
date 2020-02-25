using System.Globalization;
using ScopeState;

namespace Sample.MassTransitMiddleware.AppScopeStates
{
    public class CultureScopeState : BaseScopeState
    {
        public CultureInfo Culture { get; set; }
    }
}