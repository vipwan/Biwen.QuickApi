using Biwen.QuickApi.Auditing;
using Biwen.QuickApi.Auditing.Internal;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Biwen.QuickApi.Test;

public class AuditTest
{

    [Fact]
    public async Task SampleTest()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        //注入httpcontext
        services.AddHttpContextAccessor();
        services.AddAuditHandler<ConsoleAuditHandler>();
        services.AddScoped<IAuditTestClass, AuditTestClass>();
        services.TryAddSingleton(typeof(AuditProxyFactory<>));


        using var scope = services.BuildServiceProvider().CreateScope();

        //模拟httpcontext user
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, "test")
                    ]
                ))
            };

        var proxy = scope.ServiceProvider.GetRequiredService<AuditProxyFactory<IAuditTestClass>>();

        var decored = proxy.Create();
        var result = decored.TestMethod("key");
        Assert.Equal("TestMethod key", result);

        await decored.TestTask2(2);
        var result2 = decored.TestMethod2(1, "234");
        await Task.Delay(1);
        Assert.True(result2 < DateTime.Now);
        await decored.TestTask();

        await Task.CompletedTask;
    }

}

public interface IAuditTestClass
{
    Task TestTask();

    DateTime TestMethod2(int random, string random2);

    Task<DateTime> TestTask2(int random);

    string TestMethod(string key);

}

public class AuditTestClass : IAuditTestClass
{
    public string TestMethod(string key)
    {
        return $"TestMethod {key}";
    }

    [AuditIgnore]
    public Task TestTask()
    {
        return Task.CompletedTask;
    }

    [AuditIgnore(IgnoreType.ReturnValue)]
    public DateTime TestMethod2(int random, string random2)
    {
        return DateTime.Now;
    }

    [AuditIgnore(IgnoreType.Parameter)]
    public Task<DateTime> TestTask2(int random)
    {
        return Task.FromResult(DateTime.Now);
    }
}