using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using NUnit.Framework;
using ScopeState.Imp;
using ScopeState.MassTransitMiddleware;

namespace IntegrationTest.ScopeState.MassTransitMiddleware
{
    public class SendMessageWithParameterlessTest
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

            _testHarness.Bus.UseScopeStateSendPipeline(_basicScopeStateAccessor);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await (_testHarness?.Stop() ?? Task.CompletedTask);
        }

        [Test]
        public async Task WhenSendMessageExecuted__TraceIdIsAddedAutomatically()
        {
            const string id = nameof(WhenSendMessageExecuted__TraceIdIsAddedAutomatically);
            var someEvent = new SomeEvent
                            {
                                Id = id
                            };
            await _testHarness.InputQueueSendEndpoint.Send(someEvent);

            Assert.True(_testHarness.Consumed.Select<SomeEvent>()
                                    .Any(message => message.Context.Headers.TryGetHeader("x-trace-id", out object _)));
        }


        [Test]
        public async Task WhenSendMessageExecuted_TraceIdAccessorHasScopeState__TraceIdIsAdded()
        {
            const string id = nameof(WhenSendMessageExecuted_TraceIdAccessorHasScopeState__TraceIdIsAdded);

            _basicScopeStateAccessor.ScopeState = new BasicScopeState()
                                                  {
                                                      TraceId = "42"
                                                  };
            var someEvent = new SomeEvent
                            {
                                Id = id
                            };
            await _testHarness.InputQueueSendEndpoint.Send(someEvent);

            Assert.True(_testHarness.Consumed.Select<SomeEvent>()
                                    .Any(message => message.Context.Message.Id == id
                                                 && message.Context.Headers.TryGetHeader("x-trace-id", out object traceIdObj)
                                                 && "42" == traceIdObj.ToString()));
        }


        [Test]
        public async Task WhenSendMessageExecuted_IfTraceIdExist__ItDoesNotReplaced()
        {
            const string id = nameof(WhenSendMessageExecuted_IfTraceIdExist__ItDoesNotReplaced);

            var someEvent = new SomeEvent
                            {
                                Id = id
                            };

            await _testHarness.InputQueueSendEndpoint.Send(someEvent, context => { context.Headers.Set("x-trace-id", "42"); });

            Assert.True(_testHarness.Consumed.Select<SomeEvent>()
                                    .Any(message => message.Context.Message.Id == id
                                                 && message.Context.Headers.TryGetHeader("x-trace-id", out object traceIdObj)
                                                 && "42" == traceIdObj.ToString()));
        }
    }
}