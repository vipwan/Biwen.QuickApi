QuickEndpoint
=====================

提供直接对MinimalApi的Endpoint的封装，方便使用。

参考用例
---------------------
该方式直接扩展了MinimalApi的Endpoint,便于无负担的拆分业务


```csharp
namespace Biwen.QuickApi.DemoWeb.Apis.Endpoints
{
    /// <summary>
    /// 测试IQuickEndpoint
    /// </summary>
    [Authorize]
    [ProducesResponseType<ProblemDetails>(400)]
    [ProducesResponseType<string>(200)]
    [EndpointGroupName("test")]
    [OpenApiMetadata("IQuickEndpoint测试", "测试IQuickEndpoint", Tags = ["Endpoints"])]
    public class HelloEndpoint : IQuickEndpoint
    {
        /// <summary>
        /// 参数绑定可以实现IReqBinder<T>自定义绑定,也可以使用AsParameters在MinimalApi中默认的绑定
        /// </summary>
        public class HelloRequest : BaseRequest<HelloRequest>//, IReqBinder<HelloRequest>
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
}

```

然后注册QuickEndpoint

```csharp
app.MapGroup("endpoints", x =>
{
    //~/endpoints/hello/hello?key=world
    x.MapMethods<HelloEndpoint>("hello/{hello}");
});

```

Api文档
---------------------

相关API文档:

[IEndpointRouteBuilderExtensions](../api/Biwen.QuickApi.IEndpointRouteBuilderExtensions.yml) &nbsp;

