# ScopeState

**Appveyor**
![AppVeyor](https://img.shields.io/appveyor/ci/ademcatamak/scopestate.svg) ![AppVeyor tests](https://img.shields.io/appveyor/tests/ademcatamak/scopestate.svg)

**Travis**
![Travis (.com)](https://travis-ci.com/AdemCatamak/scopestate.svg?branch=master)

**GitHub**
![.github/workflows/github.yml](https://github.com/AdemCatamak/scopestate/workflows/.github/workflows/github.yml/badge.svg?branch=master)


**_ScopeState && ScopeState.NetCoreDIExtensions_**

ScopeState library provides storage for data that may be necessary for your application life cycle. 

For example; when WebRequest arrives your Web-Api, you can storage information that is taken from RequestHeader via IScopeStateAccessor. Thanks to this, you do not have to pass HttpContext or HttpRequest to other layers.

In ScopeState.NetCoreDIExtensions package, there is a parameterless function that inject BasicScopeStateAccesor to system. This accessor gives BasicScopeState which has only `TraceId` property.
```
services.AddScopeStateAccessor();
```

If you want to declare your custom `ScopeState`, you can look at example below.
```
public class AppScopeState : BaseScopeState
{
    public CultureInfo Culture { get; set; }
}

public class AppScopeStateAccessor : BaseScopeStateAccessor<AppScopeState>
{
}
```
```
services.AddScopeStateAccessor<AppScopeStateAccessor, AppScopeState>();
```

**_ScopeState.WebMiddleware_**

Default middleware could work with `IScopeStateAccessor<BasicScopeState>`. This middleware check is there any information exist in `x-trace-id` RequestHeader. If header has value, header's value is set into `BasicScopeState's TraceId` otherwise random value is stored into `BasicScopeState's TraceId` 
```
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
    app.UseScopeStateMiddleware();
    ...
}
```
If you define your own ScopeState object, you should give procedure how your ScopeState does created.

```
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
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
                                                           }
    ...                                              
}
```


**_ScopeState.MassTransitMiddleware_**

ConnectPublishObserver and ConnectSendObserver take ScopeState object via IScopeStateAccessor. After than ScopeState.TraceId value is set into headers as `x-trace-id`.
```
busControl.ConnectPublishObserver(new BasicPublishObserver(provider.GetService<ILogger<BasicPublishObserver>>()));
busControl.ConnectSendObserver(new BasicSendObserver(provider.GetService<ILogger<BasicSendObserver>>()));
```

There is an example of using pipeline below if you have own ScopeState model.
```
static void PreSend(SendContext context, AppScopeState scopeState)
{
  if (context.Headers == null) return;

  string cultureName = scopeState?.Culture?.Name;
  if (!string.IsNullOrEmpty(cultureName) && !context.Headers.TryGetHeader("x-culture-name", out object _))
      context.Headers.Set("x-culture-name", cultureName);

  string traceId = scopeState?.TraceId;
  if (!string.IsNullOrEmpty(traceId) && !context.Headers.TryGetHeader("x-trace-id", out object _))
      context.Headers.Set("x-trace-id", traceId);
}

busControl.UseScopeStatePublishPipeline(provider.GetService<IScopeStateAccessor<AppScopeState>>(),
                                        PreSend);
busControl.UseScopeStateSendPipeline(provider.GetService<IScopeStateAccessor<AppScopeState>>(),
                                     PreSend);
```
In ConsumePipeline, message's `x-trace-id` header is set into ScopeState.TraceId if header is not empty. Otherwise, random value is set into ScopeState.TraceId.
```
busControl.ConnectConsumeObserver(new BasicConsumeObserver(provider.GetService<ILogger<BasicConsumeObserver>>()));
```
There is an example of using pipeline below if you have own ScopeState model.

```
busControl.UseScopeStateConsumePipeline(provider.GetService<IScopeStateAccessor<AppScopeState>>(),
                                        context =>
                                        {
                                            var cultureScopeState = new AppScopeState();
                                            if (context.Headers == null) return cultureScopeState;

                                            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
                                            if (context.Headers.TryGetHeader("x-culture-name", out object languageNameObj))
                                            {
                                                var languageName = languageNameObj.ToString();
                                                try
                                                {
                                                    cultureInfo = new CultureInfo(languageName);
                                                }
                                                catch (Exception)
                                                {
                                                    cultureInfo = CultureInfo.CurrentCulture;
                                                }
                                            }

                                            string traceId = null;
                                            if (context.Headers.TryGetHeader("x-trace-id", out object traceIdObj))
                                            {
                                                traceId = traceIdObj.ToString();
                                            }

                                            cultureScopeState.Culture = cultureInfo;
                                            if (!string.IsNullOrEmpty(traceId))
                                                cultureScopeState.TraceId = traceId;

                                            return cultureScopeState;
});

```