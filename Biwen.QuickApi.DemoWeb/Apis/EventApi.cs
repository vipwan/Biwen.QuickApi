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

        public override int Order => -1;

    }

    /// <summary>
    /// 抛出异常的Handler
    /// </summary>
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

        public override int Order => -2;

        public override bool ThrowIfError => false;

    }

    [QuickApi("event")]
    public class EventApi : BaseQuickApi<MyEvent>
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(MyEvent request)
        {
            //publish
            await PublishAsync(request);
            return IResultResponse.Content("send event");
        }
    }
}