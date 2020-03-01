using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using NUnit.Framework;
using ScopeState.Imp;
using ScopeState.MassTransitMiddleware;

namespace IntegrationTest.ScopeState.MassTransitMiddleware
{
    public class ConsumeMessageWithParameterlessTest
    {
        private class SomeEvent
        {
            public string Id { get; set; }
        }

        private InMemoryTestHarness _testHarness;
        private BasicScopeStateAccessor _basicScopeStateAccessor;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _basicScopeStateAccessor = new BasicScopeStateAccessor();
        }

        [SetUp]
        public void Setup()
        {
            _testHarness = new InMemoryTestHarness();
        }

        [TearDown]
        public void OneTimeTearDown()
        {
            _testHarness?.Dispose();
        }

        [Test]
        public async Task WhenConsumeMessageExecuted__TraceIdIsGeneratedAutomatically()
        {
            const string id = nameof(WhenConsumeMessageExecuted__TraceIdIsGeneratedAutomatically);

            _testHarness.Handler<SomeEvent>(context =>
                                            {
                                                Assert.NotNull(_basicScopeStateAccessor.ScopeState);
                                                Assert.NotNull(_basicScopeStateAccessor.ScopeState.TraceId);
                                                Assert.IsNotEmpty(_basicScopeStateAccessor.ScopeState.TraceId);

                                                return Task.CompletedTask;
                                            });
            await _testHarness.Start();
            _testHarness.Bus.UseScopeStateConsumePipeline(_basicScopeStateAccessor);

            var someEvent = new SomeEvent
                            {
                                Id = id
                            };
            await _testHarness.Bus.Publish(someEvent);

            Assert.True(_testHarness.Consumed.Select<SomeEvent>().Any(m => m.Context.Message.Id == id));
            Assert.False(_testHarness.Published.Select<Fault<SomeEvent>>().Any(m => m.Context.Message.Message.Id == id));
        }


        [Test]
        public async Task WhenSendMessageExecuted_MessageHasTraceId__ScopeStateAccessorHasTheSameTraceId()
        {
            const string id = nameof(WhenSendMessageExecuted_MessageHasTraceId__ScopeStateAccessorHasTheSameTraceId);
            const string traceId = "42";
            HandlerTestHarness<SomeEvent> handlerTestHarness = _testHarness.Handler<SomeEvent>(context =>
                                                                                               {
                                                                                                   Assert.NotNull(_basicScopeStateAccessor.ScopeState);
                                                                                                   Assert.NotNull(_basicScopeStateAccessor.ScopeState.TraceId);
                                                                                                   Assert.IsNotEmpty(_basicScopeStateAccessor.ScopeState.TraceId);
                                                                                                   Assert.AreEqual("42", _basicScopeStateAccessor.ScopeState.TraceId);

                                                                                                   return Task.CompletedTask;
                                                                                               });
            await _testHarness.Start();
            _testHarness.Bus.UseScopeStateConsumePipeline(_basicScopeStateAccessor);

            var someEvent = new SomeEvent()
                            {
                                Id = id
                            };
            await _testHarness.Bus.Publish(someEvent, context => context.Headers.Set("x-trace-id", traceId));

            Assert.True(handlerTestHarness.Consumed.Select().Any());
        }
    }
}