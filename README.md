# Biwen.QuickApi

## 项目介绍
	- 提供一种简单集成的Web Api CQRS交互模块
    - 开箱即用的 路由 和 权限 以及 Request验证体验
    - 该库是NET WebApi/Minimal Api的补充，性能≈MinimalApi，但是提供了最简单的的使用体验
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

        public override HelloApiResponse Execute(EmptyRequest request)
        {
            var hello = _service.Hello($"hello world {_httpContextAccessor.HttpContext!.Request.Path} !");
            return new HelloApiResponse
            {
                Message = hello
            };
        }
    }

```


### Step4 Enjoy !

```csharp

//通过服务调用QuickApi
app.MapGet("/fromapi", (Biwen.QuickApi.DemoWeb.Apis.Hello4Api api) =>
{
    //通过你的方式获取请求对象
    var req = new EmptyRequest();
    //验证请求对象
    var validator = req.RealValidator as IValidator<EmptyRequest>;
    if (validator != null)
    {
        var result = validator.Validate(req);
        if (!result.IsValid)
        {
            return Results.BadRequest(result.ToDictionary());
        }
    }
    //执行请求
    var x = api.Execute(new EmptyRequest());
    return Results.Ok(x);
});

//直接访问
// GET ~/hello/world/biwen
// GET ~/hello/world/biwen?name=biwen
// POST ~/hello/world/biwen


```

### OpenApi & Swagger支持

![image](https://github.com/vipwan/Biwen.QuickApi/assets/13956765/bacafe3e-14eb-44da-93b8-a84d259266e4)




