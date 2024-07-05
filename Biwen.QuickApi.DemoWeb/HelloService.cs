namespace Biwen.QuickApi.DemoWeb;


public interface IHelloService
{
    string Hello(HelloService.HelloBody helloBody);
}


/// <summary>
/// 测试服务
/// </summary>

[AutoInject<IHelloService>]
[AutoInjectKeyed<IHelloService>("hello")]
public partial class HelloService(ILogger<HelloService> logger) : IHelloService
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
