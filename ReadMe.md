# ScopeState

**Appveyor**
![AppVeyor](https://img.shields.io/appveyor/ci/ademcatamak/scopestate.svg) ![AppVeyor tests](https://img.shields.io/appveyor/tests/ademcatamak/scopestate.svg)

**Travis**
![Travis (.com)](https://travis-ci.com/AdemCatamak/scopestate.svg?branch=master)

**GitHub**
![.github/workflows/github.yml](https://github.com/AdemCatamak/scopestate/workflows/.github/workflows/github.yml/badge.svg?branch=master)


**_ScopeState && ScopeState.NetCoreDIExtensions_**

ScopeState uygulamanızın yaşam döngüsü sırasında ihtiyaç duyabileceğiniz verileri saklayıp paylaşabileceğiniz saklama alanı hizmeti sunar. 

Örnek olarak; bir WebRequest aldığınız zaman, istek başlığı üzerinden verileri elde ettiğiniz verileri HttpContext veya HttpRequest tipindeki objelerinizi katmanlar arası gezdirmenden IScopeStateAccessor aracılığı ile bu verilere ulaşabilirsiniz.

Sadece `TraceId` değerine sahip olan `BasicScopeState` tipine erişebilen `ScopeStateAccessor` kullanmak için varsayılan metodu kullanabilirsiniz. 
```
services.AddScopeStateAccessor();
```

Eğer kendi tanımladığınız `ScopeState` ile uygulamanızı çalıştırmak isterseniz aşağıdaki örneğe bakabilirsiniz.

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

Varsayılan middleware WebRequest verisinin başlığında varsa "x-trace-id" başlığını alıp ScopeState.TraceId değerine yerleştirmektedir. Eğer böyle bir başlık ile karşılaşamazsa TraceId değerini rastgele olarak oluşturur.

```
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
    app.UseScopeStateMiddleware();
    ...
}
```
Eğer kendi tanımladığınız ScopeState sınıfı varsa, bu ScopeState sınıfından bir objenin nasıl oluşturulacağını aşağıdaki gibi sisteme tanıtabilirsiniz.

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

If you do not have custom ScopeState, you can use default method. This method get ScopeState's TraceId via IScopeStateAccessor's. If it does not empty and "x-trace-id" header is not exist, process set TraceId into "x-trace-id".  
Varsayılan metod kullanıldığı zaman, publish ve send işlemlerinde, ScopeStateAccessor ile ScopeState değerine ulaşılır. TraceId değeri boş değilse bu değer, x-trace-id adı ile header bilgisinin içine yerleştirilir.
```
busControl.ConnectPublishObserver(new BasicPublishObserver(provider.GetService<ILogger<BasicPublishObserver>>()));
busControl.ConnectSendObserver(new BasicSendObserver(provider.GetService<ILogger<BasicSendObserver>>()));
```

If you have custom ScopeState, you should define own process before send operation. You can look at exaple that is below.
Eğer kendi tanımladığınız ScopeState sınıfı varsa, bu ScopeState sınıfından bir objenin mesaj gönderim (Publish / Send) anında nasıl kullanılacağı aşağıdaki örnekteki gibi karar verilebilir

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

Varsayılan metod kullanıldığı zaman, consume işleminde, x-trace-id isimli header alanı içindeki veri (yoksa random bir değer) ScopeStateAccessor içerisindeki ScopeState verisinin TraceId değerine yerleştirilir.
```
busControl.ConnectConsumeObserver(new BasicConsumeObserver(provider.GetService<ILogger<BasicConsumeObserver>>()));
```

Eğer kendi tanımladığınız ScopeState sınıfı varsa, bu ScopeState sınıfından bir objenin nasıl oluşturulacağını aşağıdaki gibi UseScopeStateConsumePipeline metodunda tanıtılabilir sisteme tanıtabilirsiniz.

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