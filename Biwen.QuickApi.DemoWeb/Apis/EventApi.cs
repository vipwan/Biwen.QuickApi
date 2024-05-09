
using Biwen.QuickApi.Events;

namespace Biwen.QuickApi.DemoWeb.Apis
{

    public class MyEvent : IEvent
    {
        public string? Message { get; set; }
    }

    public class MyEventHandler : Biwen.QuickApi.Events.EventHandler<MyEvent>
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
    public class MyEventHandler2 : Biwen.QuickApi.Events.EventHandler<MyEvent>
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

        public override int Order => -1;

    }

    /// <summary>
    /// 抛出异常的Handler
    /// </summary>
    public class MyEventHandler3 : Biwen.QuickApi.Events.EventHandler<MyEvent>
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

        public override int Order => -2;

        public override bool ThrowIfError => true;

    }




    [QuickApi("event")]
    public class EventApi : BaseQuickApi
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await PublishAsync(new MyEvent { Message = "hello world!" });
            return IResultResponse.Content("send event");
        }
    }
}