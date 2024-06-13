---
_disableNewTab: true
_lang: zh-CN
_layout: landing
_appLogoUrl: https://github.com/vipwan/Biwen.QuickApi
---

# Biwen.QuickApi

![Nuget](https://img.shields.io/nuget/v/Biwen.QuickApi)
![Nuget](https://img.shields.io/nuget/dt/Biwen.QuickApi)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/vipwan/Biwen.QuickApi/blob/master/LICENSE.txt) 
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/vipwan/Biwen.QuickApi/pulls) 

## 项目介绍
Biwen.QuickApi 2+,是一个微型`aspnetcore`开发框架,提供minimalapi的QuickApi封装,提供`IQuickEndpoint`书写minimalapi,
模块化支持`Modular`,发布订阅:`IEvent`,作业调度:`IScheduleTask`,LocalLock,OpenApi ~~

```csharp
public class MyStore
{
    public static Todo[] SampleTodos()
    {
        return [
            new(1, "Walk the dog"),
            new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
            ];
    }
}

[QuickApi("todos")] //返回对象方式
public class TodoApi : BaseQuickApi<EmptyRequest,Todo[]>
{
    public override async ValueTask<Todo[]> ExecuteAsync(EmptyRequest request)
    {
        await Task.CompletedTask;
        return MyStore.SampleTodos();
    }
}
```
- (MinimalApi as REPR) Biwen.QuickApi遵循了 REPR 设计 （Request-Endpoint-Response）
- 开箱即用
- write less, do more ; write anywhere, do anything  
- 欢迎小伙伴们star&issue共同学习进步 [Biwen.QuickApi](https://github.com/vipwan/Biwen.QuickApi)

## 开发工具

- [Visual Studio 2022 17.10.0 +](https://learn.microsoft.com/zh-cn/visualstudio/releases/2022/release-notes-preview)
- [Net 9.0.0](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)

## 依赖环境&库

- Microsoft.AspNetCore.App
- [FluentValidation.AspNetCore](https://www.nuget.org/packages/FluentValidation.AspNetCore/11.3.0)
- [Microsoft.AspNetCore.OpenApi](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/8.0.5)