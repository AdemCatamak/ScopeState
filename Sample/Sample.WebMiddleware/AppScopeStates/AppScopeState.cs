using System.Globalization;
using ScopeState;

namespace SampleApi.AppScopeStates
{
    public class AppScopeState : BaseScopeState
    {
        public CultureInfo Culture { get; set; }
    }
}