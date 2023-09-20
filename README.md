# Biwen.QuickApi

## 项目介绍
	- 提供简单的Web Api CQRS框架

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
        public override HelloApiResponse Execute([From(RequestFrom.FromRoute)] HelloApiRequest request)
        {
            Console.WriteLine(HttpContextAccessor.HttpContext!.Request.Path.Value);

            return new HelloApiResponse
            {
                Message = $"Hello {request.Name} {HttpContextAccessor.HttpContext!.TraceIdentifier}"
            };
        }
    }


```


### Step4 Enjoy!

```csharp

// GET ~/hello/world/biwen
// GET ~/hello/world/biwen?name=biwen
// POST ~/hello/world/biwen


```
