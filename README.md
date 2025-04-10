# Biwen.QuickApi

![Nuget](https://img.shields.io/nuget/v/Biwen.QuickApi)
![Nuget](https://img.shields.io/nuget/dt/Biwen.QuickApi)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/vipwan/Biwen.QuickApi/blob/master/LICENSE.txt) 
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/vipwan/Biwen.QuickApi/pulls) 

## 项目介绍
[临时文档地址](http://39.108.3.212:8086/index.html) <br/>
Biwen.QuickApi 2+,是一个微型`aspnetcore`开发框架,提供minimalapi的QuickApi封装,提供`IQuickEndpoint`书写minimalapi,
模块化支持`Modular`,`多租户`,发布订阅:`IEvent`,作业调度:`IScheduleTask`,审计:`Auditing`,缓存,LocalLock,`OpenApi` ~~

## 实际应用与示例项目

### 提供MySurvey问卷调查示例项目
点击访问示例项目:[MySurvey](https://github.com/vipwan/MySurvey) - 一个基于 Biwen.QuickApi 构建的完整问卷调查系统，展示了框架在实际业务场景中的应用。

### 提供 Headless CMS模块组件
[Biwen.QuickApi.Contents](https://github.com/vipwan/Biwen.QuickApi/tree/master/extensions/Biwen.QuickApi.Contents) - 一个强大的无头CMS扩展，提供以下功能：

- 灵活的内容模型定义系统
- 多种字段类型支持（文本、Markdown、数字、日期、图片等）
- 内容版本控制和审计跟踪
- 内容渲染服务与自定义视图模板
- 通过Slug友好URL访问内容
- 完整的内容API（创建、查询、更新、删除、预览）
- 内容状态工作流（草稿、发布、归档）

## 代码示例

### 基础QuickApi示例

```c#
public class MyStore
{
    public static Todo[] SampleTodos()
    {
        return [new(1, "Walk the dog"), new(2, "Buy groceries")];
    }
}

[QuickApi("todos")] // 映射到 /api/todos
public class TodoApi : BaseQuickApi<EmptyRequest,Todo[]>
{
    public override async ValueTask<Todo[]> ExecuteAsync(EmptyRequest request)
    {
        await Task.CompletedTask;
        return MyStore.SampleTodos();
    }
}
```

### 带参数的API示例

```c#
public class CreateTodoRequest : BaseRequest<CreateTodoRequest>
{
    [Required]
    public string Title { get; set; } = null!;
    
    public bool IsCompleted { get; set; }
}

[QuickApi("todos", Verbs = Verb.POST)]
[OpenApiMetadata("创建待办事项", "添加一个新的待办事项到系统")]
public class CreateTodoApi : BaseQuickApi<CreateTodoRequest, Todo>
{
    private readonly ITodoService _todoService;
    
    public CreateTodoApi(ITodoService todoService)
    {
        _todoService = todoService;
    }
    
    public override async ValueTask<Todo> ExecuteAsync(CreateTodoRequest request, CancellationToken cancellationToken)
    {
        return await _todoService.CreateAsync(request.Title, request.IsCompleted);
    }
}
```

### 使用事件系统示例

```c#
// 定义事件
public record TodoCreatedEvent(Todo Todo) : IEvent;

// 发布事件
[QuickApi("todos", Verbs = Verb.POST)]
public class CreateTodoApi : BaseQuickApi<CreateTodoRequest, Todo>
{
    private readonly ITodoService _todoService;
    private readonly IPublisher _publisher;
    
    public CreateTodoApi(ITodoService todoService, IPublisher publisher)
    {
        _todoService = todoService;
        _publisher = publisher;
    }
    
    public override async ValueTask<Todo> ExecuteAsync(CreateTodoRequest request)
    {
        var todo = await _todoService.CreateAsync(request.Title, request.IsCompleted);
        
        // 发布事件
        await _publisher.PublishAsync(new TodoCreatedEvent(todo));
        
        return todo;
    }
}

// 处理事件
[EventHandler]
public class TodoCreatedEventHandler : IEventHandler<TodoCreatedEvent>
{
    private readonly ILogger<TodoCreatedEventHandler> _logger;
    
    public TodoCreatedEventHandler(ILogger<TodoCreatedEventHandler> logger)
    {
        _logger = logger;
    }
    
    public Task HandleAsync(TodoCreatedEvent @event)
    {
        _logger.LogInformation("新的待办事项已创建: {Title}", @event.Todo.Title);
        return Task.CompletedTask;
    }
}
```

## 设计理念与特点

- **REPR设计模式**：Biwen.QuickApi遵循 REPR 设计（Request-Endpoint-Response），使API开发结构清晰
- **开箱即用**：内置常用功能，无需额外配置即可使用
- **模块化架构**：通过模块化设计，实现功能的即插即用和业务的清晰分离
- **强大的扩展性**：通过扩展接口，可以轻松定制和扩展框架功能
- **约定优于配置**：减少配置代码，提高开发效率
- **完整的文档支持**：下载项目源代码运行`Biwen.QuickApi.DocSite`项目获取详细文档

## 社区与支持

- 欢迎小伙伴们star&issue共同学习进步 [Biwen.QuickApi](https://github.com/vipwan/Biwen.QuickApi)
- 关注作者博客: [作者博客](https://www.cnblogs.com/vipwan/)

## 开发工具

- [Visual Studio 2022 17.11.0 +](https://learn.microsoft.com/zh-cn/visualstudio/releases/2022/release-notes-preview)
- [Visual Studio Code](https://code.visualstudio.com/Download)
- [Net 9.0.0](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)

## 依赖环境&库

- Microsoft.AspNetCore.App
- [FluentValidation.DependencyInjectionExtensions](https://www.nuget.org/packages/FluentValidation.DependencyInjectionExtensions)
- [Microsoft.AspNetCore.OpenApi](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/8.0.10)
