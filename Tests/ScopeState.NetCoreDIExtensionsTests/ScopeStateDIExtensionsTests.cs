using Microsoft.Extensions.DependencyInjection;
using ScopeState.Imp;
using ScopeState.NetCoreDIExtensions;
using Xunit;

namespace ScopeState.NetCoreDIExtensionsTests
{
    public class ScopeStateDIExtensionsTests
    {
        [Fact]
        public void WhenBasicInjectionApplied__ServiceProviderShouldReturnBasicScopeStateAccessor()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddScopeStateAccessor();
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var scopeStateAccessor = serviceProvider.GetService(typeof(IScopeStateAccessor<BasicScopeState>)) as IScopeStateAccessor<BasicScopeState>;
            Assert.NotNull(scopeStateAccessor);

            var basicScopeStateAccessor = serviceProvider.GetService(typeof(BasicScopeStateAccessor)) as BasicScopeStateAccessor;
            Assert.NotNull(basicScopeStateAccessor);
        }

        [Fact]
        public void WhenAccessorInjectWithoutAs__ServiceProviderShouldReturnSelfAndInterface()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddScopeStateAccessor<NewScopeStateAccessor, BasicScopeState>();

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var scopeStateAccessor = serviceProvider.GetService(typeof(IScopeStateAccessor<BasicScopeState>)) as IScopeStateAccessor<BasicScopeState>;
            Assert.NotNull(scopeStateAccessor);

            var newScopeStateAccessor = serviceProvider.GetService(typeof(NewScopeStateAccessor)) as NewScopeStateAccessor;
            Assert.NotNull(newScopeStateAccessor);
        }

        private class NewScopeStateAccessor : BaseScopeStateAccessor<BasicScopeState>
        {
        }
    }
}