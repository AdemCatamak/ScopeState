using Microsoft.Extensions.DependencyInjection;
using ScopeState.Imp;

namespace ScopeState.NetCoreDIExtensions
{
    public static class ScopeStateDIExtensions
    {
        public static IServiceCollection AddScopeStateAccessor(this IServiceCollection serviceCollection)
        {
            AddScopeStateAccessor<BasicScopeStateAccessor, BasicScopeState>(serviceCollection);
            return serviceCollection;
        }

        public static IServiceCollection AddScopeStateAccessor<TScopeStateAccessor, TScopeState>(this IServiceCollection serviceCollection)
            where TScopeState : BaseScopeState
            where TScopeStateAccessor : class, IScopeStateAccessor<TScopeState>
        {
            serviceCollection.AddSingleton<IScopeStateAccessor<TScopeState>, TScopeStateAccessor>();
            serviceCollection.AddSingleton<TScopeStateAccessor, TScopeStateAccessor>();
            return serviceCollection;
        }
    }
}