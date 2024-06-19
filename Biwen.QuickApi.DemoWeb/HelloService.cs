namespace Biwen.QuickApi.DemoWeb;

/// <summary>
/// 测试服务
/// </summary>

[AutoInject]
[AutoInjectKeyed<HelloService>("hello")]
public class HelloService
{
    public string Hello(string name)
    {
        var str = $"Hello {name}";
        Console.WriteLine(str);
        return str;
    }
}
