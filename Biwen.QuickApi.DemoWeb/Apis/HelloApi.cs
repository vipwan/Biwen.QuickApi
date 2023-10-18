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
using System.Text.Json.Serialization;


#pragma warning disable

namespace Biwen.QuickApi.DemoWeb.Apis
{

    /// <summary>
    /// 模拟请求需要登录信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AuthRequest<T> : BaseRequest<T> where T : class, new()
    {
        [Description("登录用户名")]
        [DefaultValue("vipwan@ms.co.ltd")]
        public string? UserName { get; set; }

        [Description("登录密码")]
        public string? Password { get; set; }

    }


    public class HelloApiRequest : AuthRequest<HelloApiRequest>
    {
        public string? Name { get; set; } = "default";

        /// <summary>
        /// 别名测试
        /// </summary>
        [AliasAs("otherAlias")]
        [Description("别名测试使用:otherAlias")]
        public string? Alias { get; set; }

        [FromQuery]
        [Description("测试FromQuery:Q")]
        public string? Q { get; set; }


        /// <summary>
        /// DataAnnotations内建特性测试
        /// </summary>
        [Description("DataAnnotations内建特性 测试")]
        [StringLength(18,MinimumLength =6)]
        [EmailAddress]
        [Required]
        public string Department { get; set; }

        [Description("支持绑定querystring的[],比如tags=hello&tags=world")]
        [FromQuery]
        public string[]? Tags { get; set; }



        [FromKeyedServices("hello")]
        public HelloService HelloService { get; set; }

        public HelloApiRequest()
        {
            RuleFor(x => x.Name).NotNull().Length(2, 36);
            RuleFor(x => x.Password).NotNull().Length(2, 36);
            RuleFor(x => x.UserName).EmailAddress();//要求邮箱
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
        public async Task<HelloApiRequest> BindAsync(HttpContext context)
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
        public override async Task<HelloApiResponse> ExecuteAsync(HelloApiRequest request)
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
            builder.WithExample(new HelloApiRequest
            {
                Name = "vipwan",
                Alias = "alias",
                Q = "q54543534",
                UserName = "vipwan@ms.co.ltd",
                Password = "p234565",
                Department = "vipwan@co.ltd"
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

    }

    /// <summary>
    /// 默认不需要Group
    /// </summary>
    [QuickApi("world5", Verbs = Verb.GET)]
    [QuickApiSummary("过期测试", "过期测试",IsDeprecated =true)]
    [Obsolete("过期测试",false)]
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
    public class Hello6Api : BaseQuickApiWithoutRequest<HelloApiResponse>
    {
        public override async Task<HelloApiResponse> ExecuteAsync(EmptyRequest request)
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
    public class ContentApi : BaseQuickApi<EmptyRequest, ContentResponse>
    {
        public override Task<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult("Hello World content!".AsRspOfContent());
        }

    }

    /// <summary>
    /// JustAsService 只会被服务发现，不会被注册到路由表
    /// </summary>
    public class JustAsService : BaseQuickApiJustAsService<EmptyRequest, ContentResponse>
    {
        public override Task<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult("Hello World JustAsService!".AsRspOfContent());
        }
    }


    [QuickApi("iresult", Verbs = Verb.GET)]
    [QuickApiSummary("IResult测试", "IResult测试")]
    public class IResultTestApi : BaseQuickApiWithoutRequest<IResultResponse>
    {
        public override async Task<IResultResponse> ExecuteAsync(EmptyRequest request)
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
    public class FromBodyApi : BaseQuickApi<FromBodyRequest, ContentResponse>
    {
        public override async Task<ContentResponse> ExecuteAsync(FromBodyRequest request)
        {
            return $"FromBodyApi {request.Id} {request.Name}".AsRspOfContent();
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            return base.HandlerBuilder(builder);
        }
    }

    /// <summary>
    /// 上传文件测试
    /// 请使用postman & apifox 测试
    /// </summary>
    [QuickApi("fromfile", Verbs = Verb.POST)]
    [QuickApiSummary("上传文件测试", "上传文件测试")]
    public class FromFileApi : BaseQuickApi<FileUploadRequest, IResultResponse>
    {

        public override async Task<IResultResponse> ExecuteAsync(FileUploadRequest request)
        {
            //测试上传一个文本文件并读取内容
            if (request.File != null)
            {
                using (var sr = new StreamReader(request.File.OpenReadStream()))
                {
                    var content = await sr.ReadToEndAsync();
                    return Results.Ok(content).AsRspOfResult();
                }
            }
            return Results.BadRequest("no file").AsRspOfResult();
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.Accepts<FileUploadRequest>("multipart/form-data");
            return builder;
        }
    }

}

#pragma warning restore