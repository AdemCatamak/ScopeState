using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using ScopeState.Imp;
using ScopeState.MassTransitMiddleware;
using Xunit;

namespace ScopeState.MassTransitMiddlewareTests
{
    public class PublishMessageWithParameterlessTest : IClassFixture<MassTransitInMemoryTestHarnessFixture>
    {
        private readonly InMemoryTestHarness _testHarness;
        private readonly ConsumerTestHarness<SomeEventConsumer> _someEventConsumerTestHarness;
        private readonly BasicScopeStateAccessor _basicScopeStateAccessor;

        public PublishMessageWithParameterlessTest(MassTransitInMemoryTestHarnessFixture massTransitInMemoryTestHarnessFixture)
        {
            _basicScopeStateAccessor = new BasicScopeStateAccessor();

            _testHarness = massTransitInMemoryTestHarnessFixture.TestHarness;
            _someEventConsumerTestHarness = massTransitInMemoryTestHarnessFixture.SomeEventConsumerTestHarness;

            _testHarness.Bus.UseScopeStatePublishPipeline(_basicScopeStateAccessor);
        }

        [Fact]
        public async Task WhenPublishMessageExecuted__TraceIdIsAddedAutomatically()
        {
            var someEvent = new SomeEvent
                            {
                                SomeId = 42
                            };
            await _testHarness.Bus.Publish(someEvent);

            Assert.True(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>().Any());
            Assert.Contains(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>()
                          , message => message.Context.Headers.TryGetHeader("x-trace-id", out object _));
        }

        [Fact]
        public async Task WhenPublishMessageExecuted_TraceIdAccessorHasScopeState__TraceIdIsAdded()
        {
            _basicScopeStateAccessor.ScopeState = new BasicScopeState()
                                                  {
                                                      TraceId = "42"
                                                  };
            var someEvent = new SomeEvent
                            {
                                SomeId = 42
                            };
            await _testHarness.Bus.Publish(someEvent);

            Assert.True(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>().Any());
            Assert.Contains(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>()
                          , message => message.Context.Headers.TryGetHeader("x-trace-id", out object traceIdObj)
                                    && "42" == traceIdObj.ToString());
        }

        [Fact]
        public async Task WhenPublishMessageExecuted_IfTraceIdExist__ItDoesNotReplaced()
        {
            var someEvent = new SomeEvent
                            {
                                SomeId = 42
                            };
            await _testHarness.Bus.Publish(someEvent, context => { context.Headers.Set("x-trace-id", "42"); });

            Assert.True(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>().Any());
            Assert.Contains(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>()
                          , message => message.Context.Headers.TryGetHeader("x-trace-id", out object traceIdObj)
                                    && "42" == traceIdObj.ToString());
        }
    }
}