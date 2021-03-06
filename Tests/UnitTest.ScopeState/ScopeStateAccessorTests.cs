using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ScopeState.Imp;

namespace UnitTest.ScopeState
{
    public class ScopeStateAccessorTests
    {
        public class ScopeStateAccessorTest
        {
            [Test]
            public void WhenScopeStateSetViaAccessor__GetScopeStateShouldNotReturnNull()
            {
                string traceId = $"Trace Id:  {nameof(WhenScopeStateSetViaAccessor__GetScopeStateShouldNotReturnNull)}";

                var scopeStateAccessor = new BasicScopeStateAccessor
                                         {
                                             ScopeState =
                                                 new BasicScopeState
                                                 {
                                                     TraceId = traceId
                                                 }
                                         };

                BasicScopeState scopeState = scopeStateAccessor.ScopeState;

                Assert.AreEqual(traceId, scopeState.TraceId);
            }

            [Test]
            public async Task WhenOneScopeStateAccessorExist_AndDifferentThreadSetDifferentScopeState__EachThreadGetItOwnScopeState()
            {
                var scopeStateAccessor = new BasicScopeStateAccessor();

                var semaphore1 = new Semaphore(0, 1);
                var semaphore2 = new Semaphore(0, 1);


                var setScopeState1 = new Task(() =>
                                              {
                                                  string traceId = $"1-{nameof(WhenOneScopeStateAccessorExist_AndDifferentThreadSetDifferentScopeState__EachThreadGetItOwnScopeState)}";
                                                  scopeStateAccessor.ScopeState = new BasicScopeState
                                                                                  {
                                                                                      TraceId = traceId
                                                                                  };
                                                  semaphore1.Release();
                                                  semaphore2.WaitOne();

                                                  BasicScopeState scopeState = scopeStateAccessor.ScopeState;
                                                  Assert.AreEqual(traceId, scopeState.TraceId);
                                              });
                var setScopeState2 = new Task(() =>
                                              {
                                                  string traceId = $"2-{nameof(WhenOneScopeStateAccessorExist_AndDifferentThreadSetDifferentScopeState__EachThreadGetItOwnScopeState)}";

                                                  scopeStateAccessor.ScopeState = new BasicScopeState
                                                                                  {
                                                                                      TraceId = traceId,
                                                                                  };

                                                  semaphore2.Release();
                                                  semaphore1.WaitOne();

                                                  BasicScopeState scopeState = scopeStateAccessor.ScopeState;
                                                  Assert.AreEqual(traceId, scopeState.TraceId);
                                              });

                setScopeState1.Start();
                setScopeState2.Start();
                await Task.WhenAll(setScopeState1, setScopeState2);
            }
        }
    }
}