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
        return [new(1, "Walk the dog"),];
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

## 核心功能

Biwen.QuickApi 提供了一系列核心功能，帮助开发者快速构建高效的 Web 应用程序：

- **Minimal API 封装**：通过 `IQuickEndpoint` 快速定义 Minimal API。
- **模块化支持**：使用 `Modular` 实现功能模块化，便于扩展和维护。
- **发布订阅**：内置 `IEvent` 支持事件驱动架构。
- **作业调度**：通过 `IScheduleTask` 实现定时任务调度。
- **分布式锁**：提供 `LocalLock` 支持分布式环境下的资源锁定。
- **OpenAPI 支持**：自动生成 API 文档，便于集成和测试。

## 示例项目

- [MySurvey](https://github.com/vipwan/MySurvey)：一个基于 Biwen.QuickApi 构建的问卷调查系统，展示了框架在实际业务场景中的应用。

## 社区与支持

- **GitHub**：欢迎访问 [Biwen.QuickApi](https://github.com/vipwan/Biwen.QuickApi) 仓库，提交 Issue 或 PR。
- **博客**：关注作者博客 [万雅虎的博客](https://www.cnblogs.com/vipwan/) 获取更多资源。

## 相关文档

- [快速入门](articles/GettingStarted.md)：了解如何快速上手 Biwen.QuickApi。
- [核心概念](articles/CoreConcepts.md)：深入理解框架的设计理念和核心组件。
- [API 文档](articles/QuickApi.md)：查看完整的 API 参考文档。
- [CMS 模块](articles/CMS/Overview.md)：了解 Biwen.QuickApi.Contents 的内容管理功能。

## 开发工具

- [Visual Studio 2022 17.10.0 +](https://learn.microsoft.com/zh-cn/visualstudio/releases/2022/release-notes-preview)
- [Net 9.0.0](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)

## 依赖环境&库

- Microsoft.AspNetCore.App
- [FluentValidation.AspNetCore](https://www.nuget.org/packages/FluentValidation.AspNetCore/11.3.0)
- [Microsoft.AspNetCore.OpenApi](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/8.0.6)