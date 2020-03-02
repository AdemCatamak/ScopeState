using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Sample.MassTransitMiddleware.Consumers;

namespace Sample.MassTransitMiddleware.HostedServices
{
    public class AccountCreatedEventGeneratorService : IHostedService, IDisposable
    {
        private readonly IBusControl _busControl;
        private Timer _timer;

        public AccountCreatedEventGeneratorService(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                               TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var accountCreatedEvent1 = new AccountCreatedEvent
                                       {
                                           AccountName = Guid.NewGuid().ToString()
                                       };

            _busControl.Publish(accountCreatedEvent1, context => context.Headers.Set("x-trace-id", accountCreatedEvent1.AccountName))
                       .GetAwaiter().GetResult();

            var accountCreatedEvent2 = new AccountCreatedEvent
                                       {
                                           AccountName = Guid.NewGuid().ToString()
                                       };

            _busControl.Publish(accountCreatedEvent2, context =>
                                                      {
                                                          context.Headers.Set("x-trace-id", accountCreatedEvent2.AccountName);
                                                          context.Headers.Set("x-culture-name", "en");
                                                      })
                       .GetAwaiter().GetResult();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}