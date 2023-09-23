using Biwen.QuickApi.Attributes;
using Biwen.QuickApi.Binder;
using FluentValidation;
using System.Text.Json.Serialization;

namespace Biwen.QuickApi.DemoWeb.Apis
{

    public class HelloApiRequest : BaseRequest<HelloApiRequest>
    {
        public string? Name { get; set; } = "default";

        /// <summary>
        /// 别名测试
        /// </summary>
        [AliasAs("a")]
        public string? Alias { get; set; }

        public HelloApiRequest()
        {
            RuleFor(x => x.Name).NotNull().Length(5, 10);
        }
    }

    /// <summary>
    /// 自定义的绑定器
    /// </summary>
    public class CustomApiRequestBinder : IReqBinder<HelloApiRequest>
    {
        public async Task<HelloApiRequest> BindAsync(HttpContext context)
        {
            var request = new HelloApiRequest();

            //支持默认值,如果没有c,则使用默认值
            if (context.Request.Query.TryGetValue("c", out var c))
            {
                request.Name = c;
            }

            await Task.CompletedTask;
            return request;
        }
    }


    public class HelloApiResponse : BaseResponse
    {
        public string? Message { get; set; }

        /// <summary>
        /// 返回字段的别名测试
        /// </summary>
        ///[AliasAs("a")]
        [JsonPropertyName("a")]
        public string? Alias { get; set; }
    }


    [QuickApi("index", Group = "admin", Verbs = Verb.GET | Verb.POST, Policy = "admin")]
    public class NeedAuthApi : BaseQuickApi
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(EmptyResponse.New);
        }

        override public RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "需要验证权限的接口",
                Description = "需要验证权限的接口"
            });

            return builder;
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
                Message = $"Hello {request.Name}  {request.Alias} ",
                Alias = request.Alias //别名测试 Alias -> a
            };
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "测试别名a->Alias的绑定",
                Description = "测试别名"
            });

            return base.HandlerBuilder(builder);
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

        override public RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "模拟注入服务的接口",
                Description = "模拟注入服务的接口"
            });

            return builder;
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
    public class CustomApi : BaseQuickApi<HelloApiRequest>
    {
        public CustomApi()
        {
            UseReqBinder<CustomApiRequestBinder>();
        }

        public override async Task<EmptyResponse> ExecuteAsync(HelloApiRequest request)
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
                Summary = "模拟自定义绑定器的接口",
                Description = "模拟自定义绑定器的接口"
            });

            //自定义标签
            builder.WithTags("custom");

            //自定义过滤器
            builder.AddEndpointFilter(async (context, next) =>
            {
                Console.WriteLine("自定义过滤器!");
                return await next(context);
            });


            builder.HasApiVersion(1.0);
            return builder;
        }

    }


    #region 版本控制测试

    [QuickApi("v1")]
    public class V1Api : BaseQuickApi
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            Console.WriteLine("v1");
            return Task.FromResult(EmptyResponse.New);
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.HasApiVersion(1.0).WithGroupName("1.0");

            builder.WithTags("VT");//按照版本分组
            return builder;
        }
    }

    [QuickApi("v1v2")]
    public class V1V2Api : BaseQuickApi
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            Console.WriteLine("v1,v2");
            return Task.FromResult(EmptyResponse.New);
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {

            builder.HasApiVersion(1.0).WithGroupName("1.0");
            builder.HasApiVersion(2.0).WithGroupName("2.0");

            builder.WithTags("VT");//按照版本分组

            return builder;
        }
    }

    [QuickApi("v2")]
    public class V2Api : BaseQuickApi
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            Console.WriteLine("v2");
            return Task.FromResult(EmptyResponse.New);
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.HasApiVersion(2.0).WithGroupName("2.0");
            builder.WithTags("VT");//按照版本分组

            return builder;
        }
    }

    #endregion


    /// <summary>
    /// get ~/content 返回文本测试
    /// </summary>
    [QuickApi("content",Group = "hello", Verbs = Verb.GET)]
    public class ContentApi : BaseQuickApi<EmptyRequest, ContentResponse>
    {
        public override Task<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(new ContentResponse("Hello World content!"));
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "这个接口返回纯文本 text/plain",
                Description = "这个接口返回纯文本 text/plain"
            });

            return builder;
        }
    }

}