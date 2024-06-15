BiwenQuickApiOptions
=====================

系统配置项 [BiwenQuickApiOptions](../api/Biwen.QuickApi.BiwenQuickApiOptions.yml)

硬编码的方式配置
---------------------
```csharp

builder.Services.AddBiwenQuickApis(o =>
{
	o.RoutePrefix = "quick";//默认QuickApi的路由前缀
	o.EnableAntiForgeryTokens = true;//默认启动防伪
	o.EnablePubSub = true;//默认启动发布订阅
	o.EnableScheduling = true;//默认启动定时任务
	o.UseQuickApiExceptionResultBuilder = true;//默认为false,这里设直true是为了方便调试!
});

```
通过配置文件配置
---------------------

```json
{
  "$schema": "../quickapi-schema.json",

  "BiwenQuickApi": {
    "QuickApi": {
      "RoutePrefix": "quick",
      "EnableAntiForgeryTokens": true,
      "EnablePubSub": true,
      "EnableScheduling": true,
      "UseQuickApiExceptionResultBuilder": true
    },
    "Schedules": [
      {
        "ScheduleTaskType": "定时作业1",
        "Cron": "0/5 * * * *",
        "Description": "Every 5 mins",
        "IsAsync": true,
        "IsStartOnInit": false
      },
      {
        "ScheduleTaskType": "定时作业2",
        "Cron": "0/10 * * * *",
        "Description": "Every 10 mins",
        "IsAsync": false,
        "IsStartOnInit": true
      }
    ]
  }
}

```

注意事项
---------------------


> [!WARNING]
> 请注意`UseQuickApiExceptionResultBuilder`=`true`时,会返回异常堆栈`StackTrace`的详细信息,因此建议只在开发阶段配置.


```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "抛出一个异常!",
  "Status": 500,
  "CurrentUser": "vipwan",
  "Exception": {
    "message": "抛出一个异常!",
    "stackTrace": "   at Biwen.QuickApi.DemoWeb.Apis.ThrowApi.ExecuteAsync(EmptyRequest request) in C:\\Users\\~\\Biwen.QuickApi.DemoWeb\\Apis\\EventApi.cs:line 120\r\n   at CallSite.Target(Closure, CallSite, Object)\r\n   at Biwen.QuickApi.ServiceRegistration.ProcessRequestAsync(IHttpContextAccessor ctx, Type apiType, QuickApiAttribute quickApiAttribute) in C:\\Users\\~~\\Biwen.QuickApi\\Biwen.QuickApi\\ServiceRegistration.cs:line 438"
  },
  "RequestPath": "/quick/throw",
  "Method": "GET",
  "QueryString": "",
  "traceId": "00-9057c9916b379b56afc2c06e5244dd81-759630be67eb6307-00"
}
```

API文档
---------------------

相关API文档:
[BiwenQuickApiOptions](../api/Biwen.QuickApi.BiwenQuickApiOptions.yml) &nbsp;
[ServiceRegistration](../api/Biwen.QuickApi.ServiceRegistration.yml)&nbsp;
