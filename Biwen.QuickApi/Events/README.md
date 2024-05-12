- `EventSubscriberAttribute` & `IEventSubscriber<T>`              
EventSubscriberAttribute用于标记事件处理器,执行顺序,是否异步,以及是否抛出异常.
`IEventSubscriber<T>` 用于定义事件处理器接口,继承该接口,实现HandleAsync方法即可.可以在一个处理器中实现多个事件的处理.


- 使用方式1,调用QuickApi的PublishAsync():
```csharp
using Biwen.QuickApi.Events;
using Microsoft.AspNetCore.Mvc;

namespace Biwen.QuickApi.DemoWeb.Apis
{
    public class MyEvent : BaseRequest<MyEvent>,IEvent
    {
        [FromQuery]
        public string? Message { get; set; }
    }

    public class MyEventHandler : EventSubscriber<MyEvent>
    {
        private readonly ILogger<MyEventHandler> _logger;
        public MyEventHandler(ILogger<MyEventHandler> logger)
        {
            _logger = logger;
        }

        public override Task HandleAsync(MyEvent @event, CancellationToken ct)
        {
            _logger.LogInformation($"msg 2 : {@event.Message}");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 更早执行的Handler
    /// </summary>
    public class MyEventHandler2 : EventSubscriber<MyEvent>
    {
        private readonly ILogger<MyEventHandler> _logger;
        public MyEventHandler2(ILogger<MyEventHandler> logger)
        {
            _logger = logger;
        }

        public override Task HandleAsync(MyEvent @event, CancellationToken ct)
        {
            _logger.LogInformation($"msg 1 : {@event.Message}");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 抛出异常的Handler
    /// </summary>
    [EventSubscriber(Order =-2,ThrowIfError =false)]
   public class MyEventHandler3 : EventSubscriber<MyEvent>
    {
        private readonly ILogger<MyEventHandler> _logger;
        public MyEventHandler3(ILogger<MyEventHandler> logger)
        {
            _logger = logger;
        }

        public override Task HandleAsync(MyEvent @event, CancellationToken ct)
        {
            throw new Exception("error");
        }
    }

    [QuickApi("event")]
    public class EventApi : BaseQuickApi<MyEvent>
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(MyEvent request)
        {
            //publish event
            await PublishAsync(request);
            return IResultResponse.Content("send event");
        }
    }
}

```

- 使用方式2,IEvent的PublishAsync()扩展方法:
允许使用该扩展方法在任何Service中发布事件.

```csharp
public class MyEvent : IEvent
{
    public string? Message { get; set; }
}
await new MyEvent { Message = "1234567890" }.PublishAsync();

```
