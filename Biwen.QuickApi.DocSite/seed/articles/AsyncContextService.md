异步流服务
=====================

用于在服务中传递处理异步流的服务

注册服务
---------------------

```csharp
public class MyService(AsyncContextService<User> asyncContextService)
{

    public void SetUser(User user)
	{
		asyncContextService.Set(user);
	}

    public void DoSomething()
	{
		var user = asyncContextService.Get();
		// do something
	}
}
```

参考测试
---------------------

```csharp
[Theory]
[InlineData(true)]
[InlineData(false)]
public void TestAsyncContextService(bool inHttpContext)
{
    var services = new ServiceCollection();
    services.AddAsyncStateHttpContext();
    services.AddSingleton(typeof(AsyncContextService<>));
    var provider = services.BuildServiceProvider();

    var _httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    if (inHttpContext)
    {
        _httpContextAccessor.HttpContext = new DefaultHttpContext();
    }

    var asyncContextService = provider.GetRequiredService<AsyncContextService<User>>();
    var user = new User { ThreadId = Environment.CurrentManagedThreadId, Name = "Hello" };

    asyncContextService.Set(user);

    var raw = asyncContextService.Get();
    testOutputHelper.WriteLine($"设置前:" + raw?.ToString());
    testOutputHelper.WriteLine(Environment.NewLine);

    Assert.NotNull(raw);
}
```

API文档
---------------------
相关API文档:

[AsyncContextService](../api/Biwen.QuickApi.Infrastructure.AsyncContextService-1.yml) &nbsp;