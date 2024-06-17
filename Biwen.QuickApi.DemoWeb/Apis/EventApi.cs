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

        public override async ValueTask<IResult> ExecuteAsync(MyEvent request, CancellationToken cancellationToken)
        {
            //publish
            await PublishAsync(request, default);
            //publish event2
            await PublishAsync(new MyEvent2 { Message = "hello event2" }, cancellationToken);
            //可以使用IEvent扩展方法
            await new MyEvent { Message = "1234567890" }.PublishAsync(cancellationToken);
            return Results.Content("send event");
        }
    }

    [QuickApi("throw")]
    public class ThrowApi : BaseQuickApi
    {
        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new Exception("抛出一个异常!");
            //return Results.InternalServerError();
        }
    }


    [QuickApi("cancel")]
    public class CancelApi : BaseQuickApi
    {
        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
        {
            var taskJob = Task.Run(async () =>
             {
                 //模拟操作需要5秒的长时间业务操作
                 await Task.Delay(5000);

                 //因为taskCancel线程会在提前调用CancelAsync取消,所以这里根本不会执行!
                 return Results.Content("Done!");

             }, CancellationToken.None);

            var taskCancel = Task.Run(async () =>
            {
                //模拟1秒后取消
                await Task.Delay(1000);
                //取消任务
                await CancelAsync();

            }, CancellationToken.None);

            Task.WaitAny([taskJob, taskCancel], cancellationToken: cancellationToken);

            //因为taskCancel会在1秒后调用Cancel取消,所以会抛出TaskCanceledException,
            //因此下面的代码不会执行

            await Task.CompletedTask;
            return Results.Content("cancel api");
        }
    }
}