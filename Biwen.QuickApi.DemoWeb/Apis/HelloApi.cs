using Biwen.QuickApi.Attributes;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using NSwag.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;


#pragma warning disable

namespace Biwen.QuickApi.DemoWeb.Apis
{

    /// <summary>
    /// 公共请求的部分使用接口以复用  然后使用AutoGen生成实现类
    /// </summary>
    public interface IAuthRequest
    {
        [Description("登录用户名")]
        [DefaultValue("vipwan@ms.co.ltd")]
        string? UserName { get; set; }

        [Description("登录密码")]
        string? Password { get; set; }
    }

    [AutoGen("HelloApiRequest", "Biwen.QuickApi.DemoWeb.Apis")]
    public interface IHelloApiRequest : IAuthRequest { }

    [FromBody]
    public partial class HelloApiRequest : BaseRequest<HelloApiRequest>
    {
        public string? Name { get; set; } = "default";

        /// <summary>
        /// 别名测试
        /// </summary>
        [FromQuery(Name = "otherAlias")]
        public string? Alias { get; set; }

        [FromQuery]
        [Description("测试FromQuery:Q")]
        public string? Q { get; set; }


        /// <summary>
        /// DataAnnotations内建特性测试
        /// </summary>
        [Description("DataAnnotations内建特性 测试")]
        [StringLength(18, MinimumLength = 6)]
        [EmailAddress]
        [Required]
        public string Department { get; set; }

        [Description("querystring比如?tags=hello&tags=world")]
        [FromQuery]
        public string[]? Tags { get; set; }

        [Description("querystring比如?member={\"id\":\"123\",\"userName\":\"vipwan\"}")]
        [FromQuery(Name = "member")]
        public Member? CurrentMember { get; set; }

        [Description("header比如{\"id\":\"123\",\"userName\":\"vipwan\"}")]
        [FromHeader(Name = "headmember")]
        public Member? CurrentMemberFromHeader { get; set; }

        [Description("MemberUserType枚举测试")]
        [FromQuery]
        public UserType MemberUserType { get; set; } = UserType.No1;

        [Description("值类型绑定测试")]
        [FromQuery]
        public int? OrderId { get; set; }


        public record Member(string Id, string UserName);

        /// <summary>
        /// enum测试
        /// </summary>
        public enum UserType
        {
            No1 = 1, No2 = 2, No3 = 3,
        }

        //[FromKeyedServices("hello")]
        [FromServices]
        public HelloService HelloService { get; set; }

        public HelloApiRequest()
        {
            RuleFor(x => x.Name).NotNull().Length(2, 36);
            RuleFor(x => x.Password).NotNull().Length(2, 36);
            RuleFor(x => x.UserName).EmailAddress();//要求邮箱
            //注意嵌套对象的验证,必须手动指定 When() != null 的情况
            RuleFor(x => x.CurrentMember.UserName).EmailAddress().When(x => x.CurrentMember != null);
            RuleFor(x => x.CurrentMemberFromHeader.UserName).EmailAddress().When(x => x.CurrentMemberFromHeader != null);
        }
    }

    /// <summary>
    /// 上传文件测试
    /// </summary>
    public class FileUploadRequest : BaseRequest<FileUploadRequest>
    {
        [Description("上传的文件")]
        public IFormFile? File { get; set; }


        public FileUploadRequest()
        {
            RuleFor(x => x.File).NotNull();
        }
    }

    /// <summary>
    /// 标记FromBodyReq,表示这个请求对象是FromBody的
    /// </summary>
    public class FromBodyRequest : BaseRequestFromBody<FromBodyRequest>
    {
        public int Id { get; set; }
        [StringLength(20, MinimumLength = 2)]
        public string? Name { get; set; }

        public FromBodyRequest()
        {
            RuleFor(x => x.Id).NotNull().InclusiveBetween(1, 100);//必须1~100
        }
    }


    /// <summary>
    /// 自定义的绑定器
    /// </summary>
    public class CustomApiRequestBinder : IReqBinder<HelloApiRequest>
    {
        public static ValueTask<HelloApiRequest> BindAsync(HttpContext context, ParameterInfo parameter = null!)
        {
            var request = new HelloApiRequest();

            //支持默认值,如果没有c,则使用默认值
            if (context.Request.Query.TryGetValue("c", out var c))
            {
                request.Name = c;
            }
            if (context.Request.Query.TryGetValue("q", out var q))
            {
                request.Q = q;
            }
            if (context.Request.Query.TryGetValue("u", out var u))
            {
                request.UserName = u;
            }
            if (context.Request.Query.TryGetValue("p", out var p))
            {
                request.Password = p;
            }

            //FluentValidation当前有一个bug,当父对象为null时,无法正确的验证嵌套对象
            request.CurrentMember = new HelloApiRequest.Member("1234", "viwan@co.ltd");
            request.CurrentMemberFromHeader = new HelloApiRequest.Member("2346", "");

            return ValueTask.FromResult(request);
        }
    }


    public class HelloApiResponse
    {
        public string? Message { get; set; }

        /// <summary>
        /// 返回字段的别名测试
        /// </summary>
        ///[AliasAs("a")]
        [JsonPropertyName("a")]
        public string? Alias { get; set; }
    }

    /// <summary>
    /// get ~/hello/world/{name}
    /// </summary>
    [QuickApi("world/{name}", Group = "hello", Verbs = Verb.GET | Verb.POST)]
    public class HelloApi : BaseQuickApi<HelloApiRequest, HelloApiResponse>
    {
        public override async ValueTask<HelloApiResponse> ExecuteAsync(HelloApiRequest request)
        {
            await Task.CompletedTask;
            return new HelloApiResponse
            {
                Message = $"Hello {request.Name} {request.Q}",
                Alias = request.Alias,
            };
        }

        [OpenApiOperation("Verbs", "测试多个Verbs的情况")]
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            return base.HandlerBuilder(builder);
        }

    }

    /// <summary>
    /// Post ~/hello/world2
    /// </summary>
    [QuickApi("world2", Group = "hello", Verbs = Verb.POST)]
    [QuickApiSummary("WithExample()测试", "frombody")]
    public class Hello2Api : BaseQuickApi<HelloApiRequest, HelloApiResponse>
    {
        public override async ValueTask<HelloApiResponse> ExecuteAsync(HelloApiRequest request)
        {
            await Task.CompletedTask;
            return new HelloApiResponse
            {
                Message = $"Hello {request.Name}  {request.Alias} ",
                Alias = $"{request.Alias} {request.HelloService.Hello(request.Name)} " //别名测试 Alias -> a
            };
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //如果请求是POST,可以添加Example.否则会忽略
            builder.WithExamples([new HelloApiRequest
            {
                Name = "vipwan",
                Alias = "alias",
                Q = "q54543534",
                UserName = "vipwan@ms.co.ltd",
                Password = "p234565",
                Department = "vipwan@co.ltd"
            }
            ]);

            return base.HandlerBuilder(builder);
        }


    }

    /// <summary>
    /// Post ~/hello/world3
    /// </summary>

    [QuickApi("world3", Group = "hello", Verbs = Verb.POST)]
    public class Hello3Api : BaseQuickApi<EmptyRequest, HelloApiResponse>
    {
        public override async ValueTask<HelloApiResponse> ExecuteAsync(EmptyRequest request)
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

        public override async ValueTask<HelloApiResponse> ExecuteAsync(EmptyRequest request)
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
    [QuickApiSummary("过期测试", "过期测试", IsDeprecated = true)]
    [Obsolete("过期测试", false)]
    public class Hello5Api : BaseQuickApi
    {
        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return Results.Ok();
        }
    }

    /// <summary>
    /// 默认不需要Group
    /// </summary>
    [QuickApi("world6", Verbs = Verb.GET)]
    public class Hello6Api : BaseQuickApiWithoutRequest<HelloApiResponse>
    {
        public override async ValueTask<HelloApiResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return new HelloApiResponse
            {
                Message = "Hello 6",
                Alias = "Alias"
            };
        }
    }

    /// <summary>
    /// get ~/content 返回文本测试
    /// </summary>
    [QuickApi("content", Group = "hello", Verbs = Verb.GET)]
    [QuickApiSummary("ContentApi", "ContentApi")]
    public class ContentApi : BaseQuickApi<EmptyRequest, IResultResponse>
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return IResultResponse.Content("Hello World!");
        }
    }

    /// <summary>
    /// JustAsService 只会被服务发现，不会被注册到路由表
    /// </summary>
    public class JustAsService : BaseQuickApiJustAsService<EmptyRequest, string>
    {
        public override async ValueTask<string> ExecuteAsync(EmptyRequest request)
        {
            return "Hello World content!";
        }
    }


    [QuickApi("iresult", Verbs = Verb.GET)]
    [QuickApiSummary("IResult测试", "IResult测试")]
    public class IResultTestApi : BaseQuickApiWithoutRequest<IResultResponse>
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            //return Results.Ok("Hello World IResult!").AsRsp();

            Results<ContentHttpResult, JsonHttpResult<string>> results = TypedResults.Content("Hello World IResult!");
            return results.AsRspOfResult();
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //针对IResultResponse,需要完全自定义Produces,QuickApi无法自动识别
            builder.Produces(200, typeof(string), contentType: "text/plain");
            return builder;
            //return base.HandlerBuilder(builder);
        }
    }


    [QuickApi("frombody", Verbs = Verb.POST)]
    [QuickApiSummary("frombody", "当前接口Req来自整个FormBody")]
    public class FromBodyApi : BaseQuickApi<FromBodyRequest>
    {
        public override async ValueTask<IResult> ExecuteAsync(FromBodyRequest request)
        {
            await Task.CompletedTask;
            return Results.Ok($"FromBodyApi {request.Id} {request.Name}");
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithExamples([new FromBodyRequest { Id = 100, Name = "vipwan" }]);
            return base.HandlerBuilder(builder);
        }
    }

    /// <summary>
    /// 上传文件测试
    /// 请使用postman & apifox 测试
    /// </summary>
    [QuickApi("fromfile", Verbs = Verb.POST)]
    [QuickApiSummary("上传文件测试", "上传文件测试")]
    public class FromFileApi : BaseQuickApi<FileUploadRequest>
    {

        public override async ValueTask<IResult> ExecuteAsync(FileUploadRequest request)
        {
            //测试上传一个文本文件并读取内容
            if (request.File != null)
            {
                using (var sr = new StreamReader(request.File.OpenReadStream()))
                {
                    var content = await sr.ReadToEndAsync();
                    return Results.Ok(content);
                }
            }
            return Results.BadRequest("no file");
        }

        //public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        //{
        //    builder.Accepts<FileUploadRequest>("multipart/form-data");
        //    return builder;
        //}
    }


    /// <summary>
    /// 提供对Results<,>的支持
    /// </summary>
    [QuickApi("typed-result", Verbs = Verb.GET)]
    [QuickApiSummary("TypedResult测试", "TypedResult测试")]
    public class TypedResultTestApi : BaseQuickApi<EmptyRequest, Results<BadRequest, ValidationProblem, Ok<HelloApiResponse>>>
    {
        public override async ValueTask<Results<BadRequest, ValidationProblem, Ok<HelloApiResponse>>> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;

            //返回okay
            return TypedResults.Ok(new HelloApiResponse { Alias = "hello", Message = "typed result" });
        }
    }


    [QuickApi("array-result")]
    [QuickApiSummary("ArrayResult测试", "ArrayResult测试")]
    public class ArrayTestApi : BaseQuickApi<EmptyRequest, string[]>
    {
        public override async ValueTask<string[]> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return ["hello", "biwen-quickapi"];
        }
    }
}

#pragma warning restore