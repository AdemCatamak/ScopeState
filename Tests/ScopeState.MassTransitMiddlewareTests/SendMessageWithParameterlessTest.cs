using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using ScopeState.Imp;
using ScopeState.MassTransitMiddleware;
using Xunit;

namespace ScopeState.MassTransitMiddlewareTests
{
    public class SendMessageWithParameterlessTest : IClassFixture<MassTransitInMemoryTestHarnessFixture>
    {
        private readonly InMemoryTestHarness _testHarness;
        private readonly ConsumerTestHarness<SomeEventConsumer> _someEventConsumerTestHarness;
        private readonly BasicScopeStateAccessor _basicScopeStateAccessor;

        public SendMessageWithParameterlessTest(MassTransitInMemoryTestHarnessFixture massTransitInMemoryTestHarnessFixture)
        {
            _basicScopeStateAccessor = new BasicScopeStateAccessor();

            _testHarness = massTransitInMemoryTestHarnessFixture.TestHarness;
            _someEventConsumerTestHarness = massTransitInMemoryTestHarnessFixture.SomeEventConsumerTestHarness;

            _testHarness.Bus.UseScopeStateSendPipeline(_basicScopeStateAccessor);
        }

        [Fact]
        public async Task WhenSendMessageExecuted__TraceIdIsAddedAutomatically()
        {
            var someEvent = new SomeEvent
                            {
                                SomeId = 42
                            };
            await _testHarness.InputQueueSendEndpoint.Send(someEvent);

            Assert.True(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>().Any());
            Assert.Contains(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>()
                          , message => message.Context.Headers.TryGetHeader("x-trace-id", out object _));
        }

        [Fact]
        public async Task WhenSendMessageExecuted_TraceIdAccessorHasScopeState__TraceIdIsAdded()
        {
            _basicScopeStateAccessor.ScopeState = new BasicScopeState()
                                                  {
                                                      TraceId = "42"
                                                  };
            var someEvent = new SomeEvent
                            {
                                SomeId = 42
                            };
            await _testHarness.InputQueueSendEndpoint.Send(someEvent);

            Assert.True(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>().Any());
            Assert.Contains(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>()
                          , message => message.Context.Headers.TryGetHeader("x-trace-id", out object traceIdObj)
                                    && "42" == traceIdObj.ToString());
        }

        [Fact]
        public async Task WhenSendMessageExecuted_IfTraceIdExist__ItDoesNotReplaced()
        {
            var someEvent = new SomeEvent
                            {
                                SomeId = 42
                            };

            await _testHarness.InputQueueSendEndpoint.Send(someEvent, context => { context.Headers.Set("x-trace-id", "42"); });

            Assert.True(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>().Any());
            Assert.Contains(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>()
                          , message => message.Context.Headers.TryGetHeader("x-trace-id", out object traceIdObj)
                                    && "42" == traceIdObj.ToString());
        }
    }
}