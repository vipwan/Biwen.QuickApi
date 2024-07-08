审计
=====================

Biwen.QuickApi提供基础的审计功能。

### 使用审计代理工厂

服务注入`AuditProxyFactory<TService>`, `Create`的`TService`的服务代理将自动审计,请参考如下QuickApi:

```csharp
[QuickApi("route-audit-{hello}", Group = "route", Verbs = Verb.POST)]
[OpenApiMetadata("RouteAuditApi", "对HelloService审计", Tags = ["QuickApis"])]
public class RouteAuditApi(AuditProxyFactory<IHelloService> auditProxy) : BaseQuickApi<Route2Data, Route2Data>
{
    public override async ValueTask<Route2Data> ExecuteAsync(Route2Data request, CancellationToken cancellationToken)
    {
        var helloService = auditProxy.Create();

        var hello = helloService.Hello(new HelloService.HelloBody(request.Hello ?? "viwan", 18));
        await Task.CompletedTask;
        return request;
    }
}
```
如果对于指定的方法不需要审计,你可以添加`[AuditIgnore]`忽略,切面可能会导致可能的性能开销,对于不需要审计的方法添加该特性将不会导致性能上的开销


> [!NOTE]
> 代理服务必须是注入接口才能生成代理,比如上面的用例,Create的`HelloService`必须是`IHelloService`的实现


### 使用特性`[AuditApi]`标注QuickApi和IQuickEndpoint

框架集成了对QuickApi和IQuickEndpoint的审计切面,你只需要标注`[AuditApi]`即可

```csharp
[QuickApi("Loginout", Group = "admin")]
[OpenApiMetadata("loginout", "退出登录")]
[AuditApi]
public class LoginOut : BaseQuickApiWithoutRequest<string>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public LoginOut(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<string> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        _httpContextAccessor.HttpContext?.SignOutAsync();
        return new ValueTask<string>("已经退出登录");
    }
}
```

```csharp
[ProducesResponseType<ProblemDetails>(400)]
[ProducesResponseType<string>(200)]
[EndpointGroupName("test")]
[OpenApiMetadata("IQuickEndpoint测试", "测试IQuickEndpoint"])]
[AuditApi]//审计
public class HelloEndpoint : IQuickEndpoint
{
    public class HelloRequest : BaseRequest<HelloRequest>
    {
        [Required, Length(5, 12)]
        [Description("Hello的描述")]
        [FromRoute]
        public string Hello { get; set; } = null!;

        [Description("Key的描述")]
        [Length(5, 12)]
        [FromQuery]
        public string? Key { get; set; }
    }

    public static Delegate Handler => ([AsParameters] HelloRequest request) =>
    {
        //直接返回Content
        return Results.Content($"{request.Hello}. {request.Key}");
    };

    public static Verb Verbs => Verb.POST;
}
```

### 审计处理

默认情况下审计只会打印一条消息,不会自行更多的操作,如果需要持久化等自定义操作,请实现`IAuditHandler`接口并注入:

```csharp
internal class ConsoleAuditHandler(ILogger<ConsoleAuditHandler> logger) : IAuditHandler
{
    public Task Handle(AuditInfo auditInfo)
    {
        //仅针对Public方法拦截打印
        if (auditInfo.ActionInfo?.MethodInfo?.IsPublic is true)
        {
            logger.LogTrace("AuditInfo: {@auditInfo}", auditInfo);
        }

        if (auditInfo.IsQuickApi)
        {
            logger.LogInformation("QuickApi: {@Url}", auditInfo.Url);
        }

        return Task.CompletedTask;
    }
}
```

```csharp
services.AddAuditHandler<ConsoleAuditHandler>();
```

##### 相关文档

[AuditInfo](../api/Biwen.QuickApi.Auditing.AuditInfo.yml) &nbsp;
[IAuditHandler](../api/Biwen.QuickApi.Auditing.IAuditHandler.yml)&nbsp;
[AuditProxyFactory](../api/Biwen.QuickApi.Auditing.AuditProxyFactory-1.yml)




