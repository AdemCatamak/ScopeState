using System;
using MassTransit;

namespace ScopeState.MassTransitMiddleware
{
    public static class MassTransitScopeStatePipeline
    {
        public static void UseScopeStatePublishPipeline<TScopeState>(this IBusControl busControl, IScopeStateAccessor<TScopeState> scopeStateAccessor, Action<PublishContext, TScopeState> prePublish = null)
            where TScopeState : BaseScopeState, new()
        {
            busControl.ConnectPublishObserver(new ScopeStatePublishObserver<TScopeState>(scopeStateAccessor, prePublish));
        }

        public static void UseScopeStatePublishPipeline<TScopeState>(this IBus bus, IScopeStateAccessor<TScopeState> scopeStateAccessor, Action<PublishContext, TScopeState> prePublish = null)
            where TScopeState : BaseScopeState, new()
        {
            bus.ConnectPublishObserver(new ScopeStatePublishObserver<TScopeState>(scopeStateAccessor, prePublish));
        }

        public static void UseScopeStateSendPipeline<TScopeState>(this IBusControl busControl, IScopeStateAccessor<TScopeState> scopeStateAccessor, Action<SendContext, TScopeState> preSend = null)
            where TScopeState : BaseScopeState, new()
        {
            busControl.ConnectSendObserver(new ScopeStateSendObserver<TScopeState>(scopeStateAccessor, preSend));
        }

        public static void UseScopeStateSendPipeline<TScopeState>(this IBus bus, IScopeStateAccessor<TScopeState> scopeStateAccessor, Action<SendContext, TScopeState> preSend = null)
            where TScopeState : BaseScopeState, new()
        {
            bus.ConnectSendObserver(new ScopeStateSendObserver<TScopeState>(scopeStateAccessor, preSend));
        }

        public static void UseScopeStateConsumePipeline<TScopeState>(this IBusControl busControl, IScopeStateAccessor<TScopeState> scopeStateAccessor, Func<ConsumeContext, TScopeState> generateScopeState = null)
            where TScopeState : BaseScopeState, new()
        {
            busControl.ConnectConsumeObserver(new ScopeStateConsumeObserver<TScopeState>(scopeStateAccessor, generateScopeState));
        }

        public static void UseScopeStateConsumePipeline<TScopeState>(this IBus bus, IScopeStateAccessor<TScopeState> scopeStateAccessor, Func<ConsumeContext, TScopeState> generateScopeState = null)
            where TScopeState : BaseScopeState, new()
        {
            bus.ConnectConsumeObserver(new ScopeStateConsumeObserver<TScopeState>(scopeStateAccessor, generateScopeState));
        }
    }
}