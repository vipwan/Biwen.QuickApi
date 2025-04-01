# Biwen.QuickApi

![Nuget](https://img.shields.io/nuget/v/Biwen.QuickApi)
![Nuget](https://img.shields.io/nuget/dt/Biwen.QuickApi)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/vipwan/Biwen.QuickApi/blob/master/LICENSE.txt) 
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/vipwan/Biwen.QuickApi/pulls) 

## 项目介绍
[临时文档地址](http://39.108.3.212:8086/index.html) <br/>
Biwen.QuickApi 2+,是一个微型`aspnetcore`开发框架,提供minimalapi的QuickApi封装,提供`IQuickEndpoint`书写minimalapi,
模块化支持`Modular`,`多租户`,发布订阅:`IEvent`,作业调度:`IScheduleTask`,审计:`Auditing`,缓存,LocalLock,`OpenApi` ~~

## 提供MySurvey问卷调查示例项目
点击访问示例项目:[MySurvey](https://github.com/vipwan/MySurvey)


```c#
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
- 完整开发文档请下载项目源代码运行`Biwen.QuickApi.DocSite`项目
- 欢迎小伙伴们star&issue共同学习进步 [Biwen.QuickApi](https://github.com/vipwan/Biwen.QuickApi)

## 开发工具

- [Visual Studio 2022 17.11.0 +](https://learn.microsoft.com/zh-cn/visualstudio/releases/2022/release-notes-preview)
- [Visual Studio Code](https://code.visualstudio.com/Download)
- [Net 9.0.0](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)

## 依赖环境&库

- Microsoft.AspNetCore.App
- [FluentValidation.DependencyInjectionExtensions](https://www.nuget.org/packages/FluentValidation.DependencyInjectionExtensions)
- [Microsoft.AspNetCore.OpenApi](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/8.0.10)

## 使用方式

### Step0 Nuget 

```bash
dotnet add package Biwen.QuickApi
```
### Step1 UseBiwenQuickApis

#### BiwenQuickApiOptions配置项: 
- `RoutePrefix`:前缀,默认:api,
- `EnableAntiForgeryTokens`:是否启用防伪,默认:true,
- `EnablePubSub`:是否启用发布订阅,默认:true,[#17](https://github.com/vipwan/Biwen.QuickApi/issues/17)
- `EnableScheduling`:是否启用调度,默认:true,[#18](https://github.com/vipwan/Biwen.QuickApi/issues/18)
- `UseQuickApiExceptionResultBuilder`:是否启用QuickApi的规范化异常处理,默认:false,(true将返回详细的异常信息到前端.一般仅调试模式开启)

```csharp
services.AddBiwenQuickApis(Action<BiwenQuickApiOptions>? options);//add services
app.UseBiwenQuickApis();//use middleware
```
