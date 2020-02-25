using System;
using System.Threading.Tasks;
using MassTransit;
using Sample.MassTransitMiddleware.AppScopeStates;
using ScopeState;

namespace Sample.MassTransitMiddleware.Consumers
{
    public class AccountCreatedEventConsumer : IConsumer<AccountCreatedEvent>
    {
        private readonly IScopeStateAccessor<CultureScopeState> _scopeStateAccessor;

        public AccountCreatedEventConsumer(IScopeStateAccessor<CultureScopeState> scopeStateAccessor)
        {
            _scopeStateAccessor = scopeStateAccessor;
        }

        public Task Consume(ConsumeContext<AccountCreatedEvent> context)
        {
            Console.WriteLine($"AccountName: {context.Message.AccountName}{Environment.NewLine}" +
                              $"TraceId: {_scopeStateAccessor.ScopeState?.TraceId}{Environment.NewLine}" +
                              $"CultureName: {_scopeStateAccessor.ScopeState?.Culture?.Name}"
                             );

            return Task.CompletedTask;
        }
    }
}