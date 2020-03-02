using System.Globalization;
using ScopeState;

namespace Sample.MassTransitMiddleware.AppScopeStates
{
    public class AppScopeState : BaseScopeState
    {
        public CultureInfo Culture { get; set; }
    }
}