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
                                            var basicScopeState = new BasicScopeState
                                                                  {
                                                                      TraceId = $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}--{httpContext.Connection?.Id ?? Guid.NewGuid().ToString()}"
                                                                  };
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
            app.Use(async (httpContext, next) =>
                    {
                        IScopeStateAccessor<TScopeState> scopeStateAccessor = resolveScopeStateAccessor.Invoke(httpContext.RequestServices);
                        TScopeState scopeState = generateScopeState(httpContext);
                        scopeStateAccessor.ScopeState = scopeState;
                        await next();
                    });

            return app;
        }
    }
}