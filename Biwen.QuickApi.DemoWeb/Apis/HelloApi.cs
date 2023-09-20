using Biwen.QuickApi.Attributes;
using FluentValidation;

namespace Biwen.QuickApi.DemoWeb.Apis
{

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


    /// <summary>
    /// Post ~/hello/world2
    /// </summary>
    [QuickApi("world2", Group = "hello", Verbs = Verb.POST)]
    public class Hello2Api : BaseQuickApi<HelloApiRequest, HelloApiResponse>
    {
        public override HelloApiResponse Execute([From(RequestFrom.FromBody)] HelloApiRequest request)
        {
            Console.WriteLine(HttpContextAccessor.HttpContext!.Request.Path.Value);

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
        public override HelloApiResponse Execute(EmptyRequest request)
        {
            Console.WriteLine(HttpContextAccessor.HttpContext!.Request.Path.Value);

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
        public Hello4Api(HelloService service)
        {
            _service = service;
        }

        public override HelloApiResponse Execute(EmptyRequest request)
        {
            Console.WriteLine(HttpContextAccessor.HttpContext!.Request.Path.Value);

            var hello = _service.Hello("hello world!");
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
        public override EmptyResponse Execute(EmptyRequest request)
        {
            return EmptyResponse.Instance;
        }
    }

    /// <summary>
    /// 默认不需要Group
    /// </summary>
    [QuickApi("world6", Verbs = Verb.GET)]
    public class Hello6Api : BaseQuickApi
    {
        public override EmptyResponse Execute(EmptyRequest request)
        {
            return EmptyResponse.Instance;
        }
    }
}