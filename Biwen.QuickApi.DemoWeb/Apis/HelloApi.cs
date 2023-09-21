using Biwen.QuickApi.Attributes;
using FluentValidation;

namespace Biwen.QuickApi.DemoWeb.Apis
{

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

        /// <summary>
        /// 自定义绑定
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<CustomApiRequest> BindAsync(HttpContext context)
        {
            var @default = await base.BindAsync(context);

            //自定义绑定
            if (context.Request.Query.ContainsKey("c"))
            {
                @default.Name = context.Request.Query["c"];
            }

            return @default;
        }
    }


    public class HelloApiResponse : BaseResponse
    {
        public string? Message { get; set; }
    }


    [QuickApi("index", Group = "admin", Verbs = Verb.GET | Verb.POST, Policy = "admin")]
    public class NeedAuthApi : BaseQuickApi
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(EmptyResponse.New);
        }
    }

    /// <summary>
    /// get ~/hello/world/{name}
    /// </summary>
    [QuickApi("world/{name}", Group = "hello", Verbs = Verb.GET | Verb.POST)]
    public class HelloApi : BaseQuickApi<HelloApiRequest, HelloApiResponse>
    {
        public override async Task<HelloApiResponse> ExecuteAsync(HelloApiRequest request)
        {
            await Task.CompletedTask;
            return new HelloApiResponse
            {
                Message = $"Hello {request.Name}"
            };
        }
    }


    /// <summary>
    /// Post ~/hello/world2
    /// </summary>
    [QuickApi("world2", Group = "hello", Verbs = Verb.POST)]
    public class Hello2Api : BaseQuickApi<HelloApiRequest, HelloApiResponse>
    {
        public override async Task<HelloApiResponse> ExecuteAsync(HelloApiRequest request)
        {
            await Task.CompletedTask;
            return new HelloApiResponse
            {
                Message = $"Hello {request.Name}"
            };
        }
    }

    /// <summary>
    /// Post ~/hello/world3
    /// </summary>

    [QuickApi("world3", Group = "hello", Verbs = Verb.POST)]
    public class Hello3Api : BaseQuickApi<EmptyRequest, HelloApiResponse>
    {
        public override async Task<HelloApiResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return new HelloApiResponse
            {
                Message = $"Hello 3"
            };
        }
    }

    /// <summary>
    /// 注入服务 ~/hello/world4
    /// </summary>

    [QuickApi("world4", Group = "hello", Verbs = Verb.GET)]
    public class Hello4Api : BaseQuickApi<EmptyRequest, HelloApiResponse>
    {
        private readonly HelloService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public Hello4Api(HelloService service, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<HelloApiResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            var hello = _service.Hello($"hello world {_httpContextAccessor.HttpContext!.Request.Path} !");
            return new HelloApiResponse
            {
                Message = hello
            };
        }
    }


    /// <summary>
    /// 默认不需要Group
    /// </summary>
    [QuickApi("world5", Verbs = Verb.GET)]
    public class Hello5Api : BaseQuickApi
    {
        public override async Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return EmptyResponse.New;
        }
    }

    /// <summary>
    /// 默认不需要Group
    /// </summary>
    [QuickApi("world6", Verbs = Verb.GET)]
    public class Hello6Api : BaseQuickApi
    {
        public override async Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return EmptyResponse.New;
        }
    }

    /// <summary>
    /// get ~/custom?c=11112222
    /// </summary>
    [QuickApi("custom", Verbs = Verb.GET)]
    public class CustomApi : BaseQuickApi<CustomApiRequest>
    {
        public override async Task<EmptyResponse> ExecuteAsync(CustomApiRequest request)
        {
            await Task.CompletedTask;
            Console.WriteLine($"获取自定义的 CustomApi:,从querystring:c绑定,{request.Name}");
            return EmptyResponse.New;
        }
    }
}