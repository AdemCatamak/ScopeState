using System;
using System.Globalization;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.MassTransitMiddleware.AppScopeStates;
using Sample.MassTransitMiddleware.Consumers;
using Sample.MassTransitMiddleware.HostedServices;
using Sample.MassTransitMiddleware.MassTransitObservers;
using ScopeState;
using ScopeState.MassTransitMiddleware;
using ScopeState.NetCoreDIExtensions;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace Sample.MassTransitMiddleware
{
    class Program
    {
        static void Main()
        {
            using (IHost host = CreateHost())
            {
                host.Run();
            }
        }

        private static IHost CreateHost()
        {
            return new HostBuilder()
                  .ConfigureServices((hostContext, services) =>
                                     {
                                         services.Configure<HostOptions>(option => { option.ShutdownTimeout = TimeSpan.FromSeconds(20); });
                                         InjectDependencies(services);
                                     })
                  .ConfigureLogging((host, logging) =>
                                    {
                                        logging.SetMinimumLevel(LogLevel.Information);
                                        logging.ClearProviders();
                                        logging.AddConsole();
                                    })
                  .Build();
        }

        private static void InjectDependencies(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHostedService<BusHostedService>();
            serviceCollection.AddHostedService<AccountCreatedEventGeneratorService>();

            serviceCollection.AddScopeStateAccessor<AppScopeStateAccessor, AppScopeState>();

            serviceCollection.AddMassTransit(cfg =>
                                             {
                                                 cfg.AddConsumers(typeof(Program).Assembly);

                                                 cfg.AddBus(provider =>
                                                            {
                                                                IBusControl busControl = Bus.Factory.CreateUsingInMemory(inMemoryBusFactoryConfigurator =>
                                                                                                                         {
                                                                                                                             inMemoryBusFactoryConfigurator.ReceiveEndpoint("account-created-event-consumer",
                                                                                                                                                                            e => e.Consumer<AccountCreatedEventConsumer>(provider));
                                                                                                                         }
                                                                                                                        );

                                                                busControl.ConnectPublishObserver(new BasicPublishObserver(provider.GetService<ILogger<BasicPublishObserver>>()));
                                                                busControl.ConnectSendObserver(new BasicSendObserver(provider.GetService<ILogger<BasicSendObserver>>()));
                                                                busControl.ConnectConsumeObserver(new BasicConsumeObserver(provider.GetService<ILogger<BasicConsumeObserver>>()));

                                                                static void PreSend(SendContext context, AppScopeState scopeState)
                                                                {
                                                                    if (context.Headers == null) return;

                                                                    string cultureName = scopeState?.Culture?.Name;
                                                                    if (!string.IsNullOrEmpty(cultureName) && !context.Headers.TryGetHeader("x-culture-name", out object _))
                                                                        context.Headers.Set("x-culture-name", cultureName);

                                                                    string traceId = scopeState?.TraceId;
                                                                    if (!string.IsNullOrEmpty(traceId) && !context.Headers.TryGetHeader("x-trace-id", out object _))
                                                                        context.Headers.Set("x-trace-id", traceId);
                                                                }

                                                                busControl.UseScopeStatePublishPipeline(provider.GetService<IScopeStateAccessor<AppScopeState>>(),
                                                                                                        PreSend);
                                                                busControl.UseScopeStateSendPipeline(provider.GetService<IScopeStateAccessor<AppScopeState>>(),
                                                                                                     PreSend);
                                                                busControl.UseScopeStateConsumePipeline(provider.GetService<IScopeStateAccessor<AppScopeState>>(),
                                                                                                        context =>
                                                                                                        {
                                                                                                            var cultureScopeState = new AppScopeState();
                                                                                                            if (context.Headers == null) return cultureScopeState;

                                                                                                            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
                                                                                                            if (context.Headers.TryGetHeader("x-culture-name", out object languageNameObj))
                                                                                                            {
                                                                                                                var languageName = languageNameObj.ToString();
                                                                                                                try
                                                                                                                {
                                                                                                                    cultureInfo = new CultureInfo(languageName);
                                                                                                                }
                                                                                                                catch (Exception)
                                                                                                                {
                                                                                                                    cultureInfo = CultureInfo.CurrentCulture;
                                                                                                                }
                                                                                                            }

                                                                                                            string traceId = null;
                                                                                                            if (context.Headers.TryGetHeader("x-trace-id", out object traceIdObj))
                                                                                                            {
                                                                                                                traceId = traceIdObj.ToString();
                                                                                                            }

                                                                                                            cultureScopeState.Culture = cultureInfo;
                                                                                                            if (!string.IsNullOrEmpty(traceId))
                                                                                                                cultureScopeState.TraceId = traceId;

                                                                                                            return cultureScopeState;
                                                                                                        });

                                                                return busControl;
                                                            });
                                             }
                                            );
        }
    }
}