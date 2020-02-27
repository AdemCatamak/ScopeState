using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using ScopeState.Imp;
using ScopeState.MassTransitMiddleware;
using Xunit;

namespace ScopeState.MassTransitMiddlewareTests
{
    public class ConsumeMessageWithParameterlessTest : IClassFixture<MassTransitInMemoryTestHarnessFixture>
    {
        private readonly InMemoryTestHarness _testHarness;
        private readonly ConsumerTestHarness<SomeEventConsumer> _someEventConsumerTestHarness;
        private readonly BasicScopeStateAccessor _basicScopeStateAccessor;

        public ConsumeMessageWithParameterlessTest(MassTransitInMemoryTestHarnessFixture massTransitInMemoryTestHarnessFixture)
        {
            _basicScopeStateAccessor = new BasicScopeStateAccessor();

            _testHarness = massTransitInMemoryTestHarnessFixture.TestHarness;
            _someEventConsumerTestHarness = massTransitInMemoryTestHarnessFixture.SomeEventConsumerTestHarness;

            _testHarness.Bus.UseScopeStateConsumePipeline(_basicScopeStateAccessor);
        }

        [Fact]
        public async Task WhenConsumeMessageExecuted__TraceIdIsGeneratedAutomatically()
        {
            _testHarness.Handler<SomeEvent>(context =>
                                            {
                                                if (_basicScopeStateAccessor.ScopeState == null)
                                                    throw new ApplicationException($"{nameof(_basicScopeStateAccessor)}.{nameof(_basicScopeStateAccessor.ScopeState)} is null");

                                                if (_basicScopeStateAccessor.ScopeState.TraceId == null)
                                                    throw new ApplicationException($"{nameof(_basicScopeStateAccessor)}.{nameof(_basicScopeStateAccessor.ScopeState)}.{nameof(_basicScopeStateAccessor.ScopeState.TraceId)} is null");

                                                return Task.CompletedTask;
                                            });

            var someEvent = new SomeEvent();
            await _testHarness.Bus.Publish(someEvent);

            Assert.True(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>().Any());
            Assert.False(_someEventConsumerTestHarness.Consumed.Select<Fault<SomeEvent>>().Any());
        }


        [Fact]
        public async Task WhenSendMessageExecuted_MessageHasTraceId__ScopeStateAccessorHasTheSameTraceId()
        {
            const string traceId = "42";
            _testHarness.Handler<SomeEvent>(context =>
                                            {
                                                if (_basicScopeStateAccessor.ScopeState == null)
                                                    throw new ApplicationException($"{nameof(_basicScopeStateAccessor)}.{nameof(_basicScopeStateAccessor.ScopeState)} is null");

                                                if (_basicScopeStateAccessor.ScopeState.TraceId == null)
                                                    throw new ApplicationException($"{nameof(_basicScopeStateAccessor)}.{nameof(_basicScopeStateAccessor.ScopeState)}.{nameof(_basicScopeStateAccessor.ScopeState.TraceId)} is null");

                                                if (_basicScopeStateAccessor.ScopeState.TraceId != traceId)
                                                    throw new ApplicationException($"{nameof(_basicScopeStateAccessor)}.{nameof(_basicScopeStateAccessor.ScopeState)}.{nameof(_basicScopeStateAccessor.ScopeState.TraceId)} is not {traceId}");

                                                return Task.CompletedTask;
                                            });

            var someEvent = new SomeEvent();
            await _testHarness.Bus.Publish(someEvent, context => context.Headers.Set("x-trace-id", traceId));

            Assert.True(_someEventConsumerTestHarness.Consumed.Select<SomeEvent>().Any());
            Assert.False(_someEventConsumerTestHarness.Consumed.Select<Fault<SomeEvent>>().Any());
        }
    }
}