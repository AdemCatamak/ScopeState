using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using ScopeState.Imp;

namespace ScopeState.WebMiddleware
{
    public static class ScopeStateMiddlewareExtension
    {
        public static IApplicationBuilder UseScopeStateMiddleware(this IApplicationBuilder app)
        {
            app.UseScopeStateMiddleware(provider => provider.GetRequiredService<IScopeStateAccessor<BasicScopeState>>(),
                                        httpContext =>
                                        {
                                            var basicScopeState = new BasicScopeState();
                                            if (httpContext.Request.Headers.TryGetValue("x-trace-id", out StringValues traceId))
                                            {
                                                basicScopeState.TraceId = traceId;
                                            }

                                            return basicScopeState;
                                        });
            return app;
        }

        public static IApplicationBuilder UseScopeStateMiddleware<TScopeState>(this IApplicationBuilder app,
                                                                               Func<IServiceProvider, IScopeStateAccessor<TScopeState>> resolveScopeStateAccessor,
                                                                               Func<HttpContext, TScopeState> generateScopeState)
            where TScopeState : BaseScopeState
        {
            app.Use(async (context, next) =>
                    {
                        IScopeStateAccessor<TScopeState> scopeStateAccessor = resolveScopeStateAccessor.Invoke(context.RequestServices);
                        TScopeState scopeState = generateScopeState(context);
                        scopeStateAccessor.ScopeState = scopeState;
                        await next();
                    });

            return app;
        }
    }
}