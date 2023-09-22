# Biwen.QuickApi

## 项目介绍
```csharp
[QuickApi("hello/world")]
public class MyApi : BaseQuickApi{}
```
	- 提供一种简单集成的Minimal Web Api交互模块
    - 开箱即用的Api路由 和 权限,Bind,validator体验
    - 该库是NET WebApi/Minimal Api的补充，性能≈MinimalApi,遥遥领先于MVC和WebApi，但是提供了最简单的的使用体验
    - write less, do more ; write anywhere, do anything
## 使用方式

### Step1 UseBiwenQuickApis

```csharp

builder.Services.AddBiwenQuickApis();
//....
app.MapBiwenQuickApis();

```

### Step2 Define Request and Response

```csharp

    public class HelloApiRequest : BaseRequest<HelloApiRequest>
    {
        public string? Name { get; set; }

        public HelloApiRequest()
        {
            RuleFor(x => x.Name).NotNull().Length(5, 10);
        }
    }

    /// <summary>
    /// 模拟自定义绑定的Request
    /// </summary>
    public class CustomApiRequest : BaseRequest<CustomApiRequest>
    {
        public string? Name { get; set; }

        public CustomApiRequest()
        {
            RuleFor(x => x.Name).NotNull().Length(5, 10);
        }
    }

    /// <summary>
    /// 自定义的绑定器
    /// </summary>
    public class CustomApiRequestBinder : IReqBinder<CustomApiRequest>
    {
        public async Task<CustomApiRequest> BindAsync(HttpContext context)
        {
            var request = new CustomApiRequest
            {
                Name = context.Request.Query["c"]
            };
            await Task.CompletedTask;
            return request;
        }
    }

    public class HelloApiResponse : BaseResponse
    {
        public string? Message { get; set; }
    }

```

### Step3 Define QuickApi

```csharp

    /// <summary>
    /// get ~/admin/index
    /// </summary>
    [QuickApi("index", Group = "admin", Verbs = Verb.GET | Verb.POST, Policy = "admin")]
    public class NeedAuthApi : BaseQuickApi
    {
        public override EmptyResponse Execute(EmptyRequest request)
        {
            return EmptyResponse.Instance;
        }
    }

    /// <summary>
    /// get ~/hello/world/{name}
    /// </summary>
    [QuickApi("world/{name}", Group = "hello", Verbs = Verb.GET | Verb.POST)]
    public class HelloApi : BaseQuickApi<HelloApiRequest, HelloApiResponse>
    {
        private readonly HelloService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Hello4Api(HelloService service,IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public override HelloApiResponse Execute(HelloApiRequest request)
        {
            var hello = _service.Hello($"hello world {_httpContextAccessor.HttpContext!.Request.Path} !");
            return new HelloApiResponse
            {
                Message = hello
            };
        }
    }

    /// <summary>
    /// get ~/custom?c=11112222
    /// </summary>
    [QuickApi("custom", Verbs = Verb.GET)]
    public class CustomApi : BaseQuickApi<CustomApiRequest>
    {
        public CustomApi()
        {
            UseReqBinder<CustomApiRequestBinder>();
        }

        public override async Task<EmptyResponse> ExecuteAsync(CustomApiRequest request)
        {
            await Task.CompletedTask;
            Console.WriteLine($"获取自定义的 CustomApi:,从querystring:c绑定,{request.Name}");
            return EmptyResponse.New;
        }

        /// <summary>
        /// 提供minimal扩展
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //自定义描述
            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "This is a summary",
                Description = "This is a description"
            });

            //自定义标签
            builder.WithTags("custom");

            //自定义过滤器
            builder.AddEndpointFilter(async (context, next) =>
            {
                Console.WriteLine("自定义过滤器!");
                return await next(context);
            });

            return builder;
        }

    }


```


### Step4 Enjoy !

```csharp

//测试其他地方调用QuickApi
app.MapGet("/fromapi", async (Biwen.QuickApi.DemoWeb.Apis.Hello4Api api) =>
{
    //通过你的方式获取请求对象
    var req = new EmptyRequest();
    //验证请求对象
    var result = req.RealValidator.Validate(req);
    if (!result.IsValid)
    {
        return Results.BadRequest(result.ToDictionary());
    }
    //执行请求
    var x = await api.ExecuteAsync(new EmptyRequest());
    return Results.Ok(x);
});

//直接访问
// GET ~/hello/world/biwen
// GET ~/hello/world/biwen?name=biwen
// POST ~/hello/world/biwen
// GET ~/custom?c=11112222

```
