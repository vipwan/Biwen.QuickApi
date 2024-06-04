# Biwen.QuickApi

![Nuget](https://img.shields.io/nuget/v/Biwen.QuickApi)
![Nuget](https://img.shields.io/nuget/dt/Biwen.QuickApi)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/vipwan/Biwen.QuickApi/blob/master/LICENSE.txt) 
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/vipwan/Biwen.QuickApi/pulls) 

## 项目介绍

```c#
public class MyStore
{
    public static Todo[] SampleTodos()
    {
        return new Todo[] {
            new(1, "Walk the dog"),
            new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
        };
    }
}

[QuickApi("todos")] //返回对象方式
public class Todo2Api : BaseQuickApi<EmptyRequest,Todo[]>
{
    public override async ValueTask<Todo[]> ExecuteAsync(EmptyRequest request)
    {
        await Task.CompletedTask;
        return MyStore.SampleTodos();
    }
}
```
- (MinimalApi as REPR) Biwen.QuickApi遵循了 REPR 设计 （Request-Endpoint-Response）
- 开箱即用的Route, Policy,Binder,validator & OpenApi支持
- 该库是NET WebApi/Minimal Api的补充，性能≈MinimalApi(gen版本=minimalApi,生成原生接口代码),遥遥领先于MVC和WebApi，但是提供了最简单的的使用体验
- write less, do more ; write anywhere, do anything  
- 欢迎小伙伴们star&issue共同学习进步 [Biwen.QuickApi](https://github.com/vipwan/Biwen.QuickApi)

## 开发工具

- [Visual Studio 2022 17.8.0 +](https://learn.microsoft.com/zh-cn/visualstudio/releases/2022/release-notes-preview)
- [Net 8.0.0](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)
- 调试Gen请在VS中安装 .NET Compiler Platform SDK 组件

## 依赖环境&库

- Microsoft.AspNetCore.App
- [FluentValidation.AspNetCore](https://www.nuget.org/packages/FluentValidation.AspNetCore/11.3.0)
- [Microsoft.AspNetCore.OpenApi](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/8.0.5)

## 使用方式

### Step0 Nuget 

```bash
dotnet add package Biwen.QuickApi
```
### Step1 UseBiwenQuickApis

#### BiwenQuickApiOptions配置项: 
- `RoutePrefix`:前缀,默认:api,
- `EnableAntiForgeryTokens`:是否启用防伪,默认:true,
- `EnablePubSub`:是否启用发布订阅,默认:true,[#17](https://github.com/vipwan/Biwen.QuickApi/issues/17)
- `EnableScheduling`:是否启用调度,默认:true,[#18](https://github.com/vipwan/Biwen.QuickApi/issues/18)
- `UseQuickApiExceptionResultBuilder`:是否启用QuickApi的规范化异常处理,默认:false,(true将返回详细的异常信息到前端.一般仅调试模式开启)

```csharp
services.AddBiwenQuickApis(Action<BiwenQuickApiOptions>? options);//add services
app.UseBiwenQuickApis();//use middleware
```

### Step2 Define Request and Response

- 推荐Biwen.AutoClassGen(已内置)生成Partial Request & DTO对象 [参考代码](https://github.com/vipwan/Biwen.QuickApi/blob/master/Biwen.QuickApi.DemoWeb/Apis/AutoClassGenApi.cs)

```csharp

public class HelloApiRequest : BaseRequest<HelloApiRequest>
{
    [Description("Name Desc")]
    public string? Name { get; set; }

    /// <summary>
    /// FromQuery特性绑定字段
    /// </summary>
    [FromQuery("q")]
    public string? Q { get; set; }
    public HelloApiRequest()
    {
        RuleFor(x => x.Name).NotNull().Length(5, 10);
    }
}
    
/// <summary>
/// 上传文件FileUploadRequest 
/// </summary>
public class FileUploadRequest : BaseRequest<FileUploadRequest>
{
    public IFormFile? File { get; set; }

    public FileUploadRequest()
    {
        RuleFor(x => x.File).NotNull();
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
/// 标记FromBody,表示这个请求对象是FromBody的
/// </summary>
[FromBody]
public class FromBodyRequest : BaseRequest<FromBodyRequest>
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public FromBodyRequest()
    {
        RuleFor(x => x.Id).InclusiveBetween(1, 100);//必须1~100
    }
}
/// <summary>
/// 自定义的绑定器
/// </summary>
public class CustomApiRequestBinder : IReqBinder<CustomApiRequest>
{
    public static async ValueTask<CustomApiRequest> BindAsync(HttpContext context,ParameterInfo parameter = null)
    {
        var request = new CustomApiRequest
        {
            Name = context.Request.Query["c"]
        };
        await Task.CompletedTask;
        return request;
    }
}

public class HelloApiResponse
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
[QuickApiSummary("this is summary","this is description")]
public class NeedAuthApi : BaseQuickApi
{
    public override IResult Execute(EmptyRequest request)
    {
        return Results.Ok();
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

    public HelloApi(HelloService service,IHttpContextAccessor httpContextAccessor)
    {
        _service = service;
        _httpContextAccessor = httpContextAccessor;
    }

    public override HelloApiResponse Execute(HelloApiRequest request)
    {
        var hello = _service.Hello($"hello world {_httpContextAccessor.HttpContext!.Request.Path} !");
        return new HelloApiResponse{ Message = hello };
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
        //自定义绑定器
        UseReqBinder<CustomApiRequestBinder>();
    }

    public override async ValueTask<IResult> ExecuteAsync(CustomApiRequest request)
    {
        await Task.CompletedTask;
        Console.WriteLine($"获取自定义的 CustomApi:,从querystring:c绑定,{request.Name}");
        return Results.Ok();
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
        //默认实现了Accepts和Produces
        return base.HandlerBuilder(builder);
        //如果完全自定义直接返回Builder
        //return builder;
        }
}
    
/// <summary>
/// 上传文件测试
/// 请使用postman & apifox 测试
/// </summary>
[QuickApi("fromfile", Verbs = Verb.POST)]
[QuickApiSummary("上传文件测试", "上传文件测试")]
public class FromFileApi : BaseQuickApi<FileUploadRequest, Results<Ok<string>,BadRequest<string>>>
{
    public override async ValueTask<Results<Ok<string>,BadRequest<string>> ExecuteAsync(FileUploadRequest request)
    {
        //测试上传一个文本文件并读取内容
        if (request.File != null)
        {
            using (var sr = new StreamReader(request.File.OpenReadStream()))
            {
                var content = await sr.ReadToEndAsync();
                return TypedResults.Ok(content);
            }
        }
        return TypedResults.BadRequest("no file");
    }
}

/// <summary>
/// JustAsService 只会被服务发现，不会被注册到路由表
/// </summary>
[QuickApi(""), JustAsService]
public class JustAsService : BaseQuickApi<EmptyRequest, string>
{
    public override async ValueTask<string> ExecuteAsync(EmptyRequest request)
    {
        return "Hello World JustAsService!";
    }
}
```

### 提供QuickApi的Group扩展支持

```csharp

   // 当前模拟给所有 Group为空的QuickApi加上 Tag "Def" 
    public class MyGroupRouteBuilder : IQuickApiGroupRouteBuilder
    {
        // 表述Group为空的QuickApi
        public string Group => string.Empty;
        // 执行顺序
        public int Order => 1;
        // 实现Builder方法
        public RouteGroupBuilder Builder(RouteGroupBuilder routeBuilder)
        {
            // 给所有 Group为空的QuickApi加上 Tag "Def"
            routeBuilder.WithTags("Def");
            return routeBuilder;
        }
    }

// 最后注册
builder.Services.AddBiwenQuickApiGroupRouteBuilder<MyGroupRouteBuilder>();

```

### Step4 Enjoy

```csharp

//直接访问
// GET ~/hello/world/biwen
// GET ~/hello/world/biwen?name=biwen
// POST ~/hello/world/biwen
// GET ~/custom?c=11112222

```

```csharp

//你也可以把QuickApi当Service使用
app.MapGet("/fromapi", async (Apis.Hello4Api api) =>
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

```

### Step5 OpenApi集成

```c#

//register openapi & quickapi document
builder.Services.AddOpenApi(options =>
{
    options.UseTransformer<BearerSecuritySchemeTransformer>();
    options.ShouldInclude = (desc) => true;
});

//more doc group...

//map openapi doc & ui
app.MapGroup("openapi", app =>
{
    //swagger ui
    app.MapOpenApi("{documentName}.json");
    app.MapScalarUi();
});
```
### Step6 OpenApi 以及Client代理

- 你可以全局配置版本号,以及自定义的OpenApi描述
- 你可以重写QuickApi的HandlerBuilder方法,以便于你自定义的OpenApi描述
- 我们强烈建议您使用Refit生成代理代码,以便于您的客户端和服务端保持一致的接口定义
- 因为遵循REPR风格,所以不推荐SwaggerUI或使用SwaggerStudio生成代理代码,除非您的QuickApi定义的相当规范(如存在自定义绑定,别名绑定等)!

```csharp

/// <summary>
/// refit client
/// </summary>
public interface IBusiness
{
    [Refit.Get("/fromapi")]
    public Task<TestRsp> TestPost();
}

//Refit
builder.Services.AddRefitClient<IBusiness>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5101"));

var app = builder.Build();

app.MapGet("/from-quickapi", async (IBusiness bussiness) =>
{
    var resp = await bussiness.TestPost();
    return Results.Content(resp.Message);
});

```

### Benchmark性能测试

```txt
BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.3570/22H2/2022Update)
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
[Host]     : .NET 8.0.0 (8.0.0.100), X64 RyuJIT AVX2 [AttachedDebugger]
Job-WHDDIT : .NET 8.0.0 (8.0.0.100), X64 RyuJIT AVX2

Runtime=.NET 8.0  InvocationCount=2000  IterationCount=10  
LaunchCount=1  WarmupCount=1  

```

| Method      | Mean     | Error     | StdDev    | Median   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------ |---------:|----------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
| WebApiCtrl      | 385.5 μs | 357.93 μs | 236.75 μs | 231.0 μs |  1.00 |    0.00 | 2.5000 |   33.5 KB |        1.00 |
| MinimalApi  | 221.2 μs |  13.02 μs |   6.81 μs | 220.9 μs |  0.68 |    0.34 | 2.0000 |  24.38 KB |        0.73 |
| QuickApi    | 235.9 μs |  22.26 μs |  11.65 μs | 235.4 μs |  0.72 |    0.34 | 2.0000 |  27.59 KB |        0.82 |

### Q&A

- 为什么不支持多个参数的绑定?
-- 因为我认为这样的Api设计是不合理的,我们遵循REPR设计理念,如果你需要多个参数,请使用复杂化的Request对象

- QuickApi中如何拿到HttpContext对象?
-- 请在构造函数中注入IHttpContextAccessor获取

- 是否支持Minimal的中间件和拦截器?
-- 支持的,本身QuickApi就是扩展了MinimalApi,底层也是Minimal的处理机制,所以请考虑全局的中间件和拦截器,以及重写QuickApi的HandlerBuilder方法
-- 如果你仅仅需要使用中间件控制QuickApi的行为可以参考下面的代码:

```csharp
var md = context.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>();
if (md == null || md.QuickApiType == null)
{
    await _next(context);
    return;
}

//todo:

```
