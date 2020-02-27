using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;

namespace ScopeState.MassTransitMiddlewareTests
{
    public class MassTransitInMemoryTestHarnessFixture : IDisposable
    {
        public InMemoryTestHarness TestHarness { get; private set; }
        public ConsumerTestHarness<SomeEventConsumer> SomeEventConsumerTestHarness { get; private set; }

        public MassTransitInMemoryTestHarnessFixture()
        {
            TestHarness = new InMemoryTestHarness();
            SomeEventConsumerTestHarness = TestHarness.Consumer<SomeEventConsumer>();
            TestHarness.Start()
                       .GetAwaiter()
                       .GetResult();
        }

        public void Dispose()
        {
            TestHarness.Stop()
                       .GetAwaiter().GetResult();
            TestHarness?.Dispose();
        }
    }

    public class SomeEvent
    {
        public int SomeId { get; set; }
    }

    public class SomeEventConsumer : IConsumer<SomeEvent>
    {
        public Task Consume(ConsumeContext<SomeEvent> context)
        {
            return Task.CompletedTask;
        }
    }
}