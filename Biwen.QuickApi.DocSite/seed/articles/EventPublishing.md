# 发布订阅

Biwen.QuickApi 提供了强大的事件发布订阅系统，支持同步和异步事件处理。

## 基本概念

### 1. 事件定义

事件是一个简单的类，继承自 `IEvent` 接口：

```csharp
public class UserCreatedEvent : IEvent
{
    public int UserId { get; set; }
    public string UserName { get; set; }
}
```

### 2. 事件处理器

事件处理器实现 `IEventHandler<TEvent>` 接口：

```csharp
public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserCreatedEvent @event)
    {
        _logger.LogInformation("User {UserId} created with name {UserName}", 
            @event.UserId, @event.UserName);
        
        // 处理事件逻辑
    }
}
```

## 使用方式

### 1. 发布事件

```csharp
public class UserApi : BaseQuickApi
{
    private readonly IEventPublisher _eventPublisher;

    public UserApi(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task<Result> Create([FromBody] CreateUserDto dto)
    {
        // 创建用户逻辑...

        // 发布事件
        await _eventPublisher.PublishAsync(new UserCreatedEvent
        {
            UserId = user.Id,
            UserName = user.Name
        });

        return Result.Success();
    }
}
```

### 2. 注册事件处理器

在 `Program.cs` 中注册事件处理器：

```csharp
builder.Services.AddEventHandlers(Assembly.GetExecutingAssembly());
```

## 高级特性

### 1. 异步事件处理

事件处理器默认是异步的，支持长时间运行的任务：

```csharp
public class LongRunningEventHandler : IEventHandler<SomeEvent>
{
    public async Task HandleAsync(SomeEvent @event)
    {
        // 长时间运行的任务
        await Task.Delay(TimeSpan.FromMinutes(5));
    }
}
```

### 2. 事件过滤

可以使用 `[EventFilter]` 特性来过滤事件：

```csharp
[EventFilter(typeof(UserEventFilter))]
public class FilteredEventHandler : IEventHandler<UserEvent>
{
    // ...
}
```

### 3. 事务支持

事件发布可以与数据库事务集成：

```csharp
public class UserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public async Task CreateUser(CreateUserDto dto)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 创建用户
            var user = await _userRepository.CreateAsync(dto);
            
            // 发布事件
            await _eventPublisher.PublishAsync(new UserCreatedEvent
            {
                UserId = user.Id
            });

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

## 最佳实践

1. **保持事件轻量级**
   - 事件应该只包含必要的数据
   - 避免在事件中包含大型对象

2. **错误处理**
   - 事件处理器应该处理自己的异常
   - 使用日志记录重要信息

3. **性能考虑**
   - 对于耗时操作，使用异步事件处理
   - 考虑使用后台任务处理大量事件

## 下一步

- [作业调度](Scheduling.md)
- [UnitOfWork](UnitOfWork.md)
- [审计](Auditing.md) 