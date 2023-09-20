# Biwen.QuickApi

## 项目介绍
	- 提供一种简单的Web Api CQRS交互框架
    - 支持开箱即用的路由和权限配置以及DTO验证体验
    - 该框架是MinimalApi的补充，性能无限接近于MinimalApi，但是提供了最简单的的使用体验

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
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name不能为空");
            RuleFor(x => x.Name).MaximumLength(10).WithMessage("Name最大长度为10");
            RuleFor(x => x.Name).MinimumLength(5).WithMessage("Name最小长度为5");
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

//通过注入服务调用
app.MapGet("/fromapi", (Biwen.QuickApi.DemoWeb.Apis.Hello4Api api) =>
{
    var x = api.Execute(new EmptyRequest());
    return Results.Ok(x);
});

//直接访问
// GET ~/hello/world/biwen
// GET ~/hello/world/biwen?name=biwen
// POST ~/hello/world/biwen


```
