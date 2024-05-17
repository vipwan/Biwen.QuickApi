using Biwen.QuickApi.Events;
using Microsoft.AspNetCore.Mvc;

namespace Biwen.QuickApi.DemoWeb.Apis
{
    public class MyEvent : BaseRequest<MyEvent>, IEvent
    {
        [FromQuery]
        public string? Message { get; set; }
    }


    public class MyEvent2 : IEvent
    {
        public string? Message { get; set; }
    }


    [EventSubscriber(Order = 0, IsAsync = true)]
    public class MyEventHandler : EventSubscriber<MyEvent>
    {
        private readonly ILogger<MyEventHandler> _logger;
        public MyEventHandler(ILogger<MyEventHandler> logger)
        {
            _logger = logger;
        }

        public override async Task HandleAsync(MyEvent @event, CancellationToken ct)
        {
            //模拟异步操作
            await Task.Delay(5000, ct);

            _logger.LogInformation($"msg 2 : {@event.Message}");
        }
    }

    /// <summary>
    /// 更早执行的Handler
    /// </summary>
    [EventSubscriber(Order = 1)]
    public class MyEventHandler2 : EventSubscriber<MyEvent>
    {
        private readonly ILogger<MyEventHandler2> _logger;
        public MyEventHandler2(ILogger<MyEventHandler2> logger)
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
    [EventSubscriber(Order = -2, ThrowIfError = false)]
    public class MyEventHandler3 : EventSubscriber<MyEvent>
    {
        private readonly ILogger<MyEventHandler3> _logger;
        public MyEventHandler3(ILogger<MyEventHandler3> logger)
        {
            _logger = logger;
        }

        public override Task HandleAsync(MyEvent @event, CancellationToken ct)
        {
            throw new Exception("error");
        }
    }

    /// <summary>
    /// 同时订阅多个事件
    /// </summary>
    [EventSubscriber(IsAsync = true, Order = 0, ThrowIfError = false)]
    public class MyEventHandler4 : IEventSubscriber<MyEvent>, IEventSubscriber<MyEvent2>
    {
        private readonly ILogger<MyEventHandler4> _logger;
        public MyEventHandler4(ILogger<MyEventHandler4> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(MyEvent @event, CancellationToken ct)
        {
            _logger.LogInformation($"muti msg 1 : {@event.Message}");
            return Task.CompletedTask;
        }

        public Task HandleAsync(MyEvent2 @event, CancellationToken ct)
        {
            _logger.LogInformation($"muti msg 2 : {@event.Message}");
            return Task.CompletedTask;
        }
    }


    [QuickApi("event")]
    public class EventApi : BaseQuickApi<MyEvent>
    {

        public override async ValueTask<IResult> ExecuteAsync(MyEvent request)
        {
            //publish
            await PublishAsync(request);
            //publish event2
            await PublishAsync(new MyEvent2 { Message = "hello event2" });
            //可以使用IEvent扩展方法
            await new MyEvent { Message = "1234567890" }.PublishAsync();
            return Results.Content("send event");
        }
    }
}