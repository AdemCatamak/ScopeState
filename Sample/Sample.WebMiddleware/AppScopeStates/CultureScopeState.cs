using System.Globalization;
using ScopeState;

namespace SampleApi.AppScopeStates
{
    public class CultureScopeState : BaseScopeState
    {
        public CultureInfo Culture { get; set; }
    }
}