using static Biwen.QuickApi.DemoWeb.HelloService;

namespace Biwen.QuickApi.DemoWeb;

/// <summary>
/// 测试服务
/// </summary>

[AutoInject]
[AutoInjectKeyed<HelloService>("hello")]
public partial class HelloService(ILogger<HelloService> logger)
{
    public record HelloBody(string name, int age);



    public string Hello(HelloBody helloBody)
    {
        var str = $"Hello {helloBody.name}";
        Console.WriteLine(str);

        Log.LogInfo(logger, helloBody);
        //logger.LogInformation($"Hello {helloBody.name}");
        return str;
    }

    static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Hello {helloBody}")]
        public static partial void LogInfo(ILogger logger, [LogProperties] HelloBody helloBody);
    }

}
