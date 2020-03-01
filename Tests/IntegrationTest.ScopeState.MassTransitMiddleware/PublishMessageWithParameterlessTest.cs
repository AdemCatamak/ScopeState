using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using NUnit.Framework;
using ScopeState.Imp;
using ScopeState.MassTransitMiddleware;

namespace IntegrationTest.ScopeState.MassTransitMiddleware
{
    public class PublishMessageWithParameterlessTest
    {
        private class SomeEvent
        {
            public string Id { get; set; }
        }

        private class SomeEventConsumer : IConsumer<SomeEvent>
        {
            public Task Consume(ConsumeContext<SomeEvent> context)
            {
                return Task.CompletedTask;
            }
        }


        private InMemoryTestHarness _testHarness;
        private ConsumerTestHarness<SomeEventConsumer> _someEventConsumerTestHarness;
        private BasicScopeStateAccessor _basicScopeStateAccessor;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _basicScopeStateAccessor = new BasicScopeStateAccessor();

            _testHarness = new InMemoryTestHarness();
            _someEventConsumerTestHarness = _testHarness.Consumer<SomeEventConsumer>();

            await _testHarness.Start();

            _testHarness.Bus.UseScopeStatePublishPipeline(_basicScopeStateAccessor);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await (_testHarness?.Stop() ?? Task.CompletedTask);
        }

        [Test]
        public async Task WhenPublishMessageExecuted__TraceIdIsAddedAutomatically()
        {
            const string someEventId = nameof(WhenPublishMessageExecuted__TraceIdIsAddedAutomatically);
            var someEvent = new SomeEvent
                            {
                                Id = someEventId
                            };
            await _testHarness.Bus.Publish(someEvent);

            Assert.True(_testHarness.Consumed.Select<SomeEvent>().Any(message => message.Context.Message.Id == someEventId
                                                                              && message.Context.Headers.TryGetHeader("x-trace-id", out object _)));
        }

        [Test]
        public async Task WhenPublishMessageExecuted_TraceIdAccessorHasScopeState__TraceIdIsAdded()
        {
            const string someEventId = nameof(WhenPublishMessageExecuted_TraceIdAccessorHasScopeState__TraceIdIsAdded);

            _basicScopeStateAccessor.ScopeState = new BasicScopeState
                                                  {
                                                      TraceId = "42"
                                                  };
            var someEvent = new SomeEvent
                            {
                                Id = someEventId
                            };
            await _testHarness.Bus.Publish(someEvent);

            Assert.True(_testHarness.Consumed.Select<SomeEvent>()
                                    .Any(message => message.Context.Message.Id == someEventId
                                                 && message.Context.Headers.TryGetHeader("x-trace-id", out object traceIdObj)
                                                 && "42" == traceIdObj.ToString()));
        }

        [Test]
        public async Task WhenPublishMessageExecuted_IfTraceIdExist__ItDoesNotReplaced()
        {
            const string someEventId = nameof(WhenPublishMessageExecuted_IfTraceIdExist__ItDoesNotReplaced);

            _basicScopeStateAccessor.ScopeState = new BasicScopeState()
                                                  {
                                                      TraceId = "some-random-trace-id"
                                                  };
            var someEvent = new SomeEvent
                            {
                                Id = someEventId
                            };
            await _testHarness.Bus.Publish(someEvent, context => { context.Headers.Set("x-trace-id", "42"); });

            Assert.True(_testHarness.Consumed.Select<SomeEvent>()
                                    .Any(message => message.Context.Message.Id == someEventId
                                                 && message.Context.Headers.TryGetHeader("x-trace-id", out object traceIdObj)
                                                 && "42" == traceIdObj.ToString()));
        }
    }
}