using System;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using SampleApi.AppScopeStates;
using ScopeState;
using ScopeState.NetCoreDIExtensions;
using ScopeState.WebMiddleware;

namespace SampleApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScopeStateAccessor();
            services.AddScopeStateAccessor<AppScopeStateAccessor, AppScopeState>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseScopeStateMiddleware();

            app.UseScopeStateMiddleware<AppScopeState>(provider => provider.GetService<IScopeStateAccessor<AppScopeState>>(),
                                                           httpContext =>
                                                           {
                                                               AppScopeState appScopeState = new AppScopeState();
                                                               
                                                               CultureInfo cultureInfo = CultureInfo.InvariantCulture;
                                                               if (!httpContext.Request.Headers.TryGetValue("Accept-Language", out StringValues languageName))
                                                               {
                                                                   try
                                                                   {
                                                                       cultureInfo = new CultureInfo(languageName);
                                                                   }
                                                                   catch (Exception)
                                                                   {
                                                                       cultureInfo = CultureInfo.CurrentCulture;
                                                                   }
                                                               }

                                                               appScopeState.Culture = cultureInfo;
                                                               
                                                               if (httpContext.Request.Headers.TryGetValue("x-trace-id", out StringValues traceId))
                                                               {
                                                                   appScopeState.TraceId = traceId;
                                                               }

                                                               return appScopeState;
                                                           });


            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}