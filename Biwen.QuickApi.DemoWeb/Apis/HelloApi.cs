using Biwen.QuickApi.Attributes;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Validations.Rules;
using NSwag.Annotations;
using System.Security.Claims;
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
        public string? UserName { get; set; }
        public string? Password { get; set; }

    }


    public class HelloApiRequest : AuthRequest<HelloApiRequest>
    {
        public string? Name { get; set; } = "default";

        /// <summary>
        /// 别名测试
        /// </summary>
        [AliasAs("a")]
        public string? Alias { get; set; }

        //[JsonIgnore]//FromQuery POST无需展示字段
        [FromQuery]
        public string? Q { get; set; }

        //[JsonIgnore]//FromKeyedServices POST无需展示字段
        [FromKeyedServices("hello")]
        public HelloService HelloService { get; set; }

        public HelloApiRequest()
        {
            RuleFor(x => x.Name).NotNull().Length(2, 36);
            RuleFor(x => x.Password).NotNull().Length(2, 36);
        }
    }

    /// <summary>
    /// 上传文件测试
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
    /// 标记FromBodyReq,表示这个请求对象是FromBody的
    /// </summary>
    public class FromBodyRequest : BaseRequestFromBody<FromBodyRequest>
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

        [OpenApiOperation("world2", "world2")]
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //如果请求是POST,可以添加Example.否则会忽略
            builder.WithExample(new HelloApiRequest
            {
                Name = "vipwan",
                Alias = "alias",
                Q = "q54543534",
                UserName = "u545435",
                Password = "p234565",
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
    /// get ~/custom?c=11112&p=12345&u=1234567
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
        [OpenApiOperation("custom", "自定义绑定.系统生成的SwagDoc传参没有意义,请按照实际情况传参")]
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //自定义标签
            builder.WithTags("Custom");

            //自定义过滤器
            builder.AddEndpointFilter(async (context, next) =>
            {
                Console.WriteLine("自定义过滤器!");
                return await next(context);
            });

            //NSwag 必须使用 OpenApiOperationAttribute 
            //Swashbuckle 使用 WithOpenApi
            //builder.WithOpenApi(operation => new(operation)
            //{
            //    Summary = "custom",
            //    Description = "自定义绑定.系统生成的SwagDoc传参没有意义,请按照实际情况传参"
            //});
            return base.HandlerBuilder(builder);
        }
    }

    /// <summary>
    /// get ~/content 返回文本测试
    /// </summary>
    [QuickApi("content", Group = "hello", Verbs = Verb.GET)]
    public class ContentApi : BaseQuickApi<EmptyRequest, ContentResponse>
    {
        public override Task<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(new ContentResponse("Hello World content!"));
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            return base.HandlerBuilder(builder);
        }
    }

    /// <summary>
    /// JustAsService 只会被服务发现，不会被注册到路由表
    /// </summary>
    public class JustAsService : BaseQuickApiJustAsService<EmptyRequest, ContentResponse>
    {
        public override Task<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(new ContentResponse("Hello World JustAsService!"));
        }
    }


    [QuickApi("iresult", Verbs = Verb.GET)]
    public class IResultTestApi : BaseQuickApiWithoutRequest<IResultResponse>
    {
        public override async Task<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            //return Results.Ok("Hello World IResult!").AsRsp();

            Results<ContentHttpResult, JsonHttpResult<string>> results = TypedResults.Content("Hello World IResult!");
            return results.AsRsp();
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
    public class FromBodyApi : BaseQuickApi<FromBodyRequest, ContentResponse>
    {
        public override async Task<ContentResponse> ExecuteAsync(FromBodyRequest request)
        {
            return new ContentResponse($"FromBodyApi {request.Id} {request.Name}");
        }

        [OpenApiOperation("frombody", "当前接口Req来自整个FormBody")]
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
                    return Results.Ok(content).AsRsp();
                }
            }
            return Results.BadRequest("no file").AsRsp();
        }

        [OpenApiOperation("fromfile", "上传文件测试")]
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.Accepts<FileUploadRequest>("multipart/form-data");

            //builder.WithOpenApi(operation => new(operation)
            //{
            //    Summary = "上传文件测试",
            //    Description = "上传文件测试"
            //});
            return builder;
        }
    }

    #region 含权限的测试



    /// <summary>
    ///  模拟直接登录,并且给予admin的Policy
    /// </summary>
    [QuickApi("logined", Group = "admin")]
    public class Login : BaseQuickApiWithoutRequest<ContentResponse>
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        public Login(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            //模拟当前账号登录
            _httpContextAccessor.HttpContext.SignInAsync(
                new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "vipwan"),
                new Claim("admin", "admin"),
                new Claim(ClaimTypes.Role, "admin"),
            }, "admin")));

            return Task.FromResult(new ContentResponse("已经登录成功"));
        }


        [OpenApiOperation("logined", "模拟直接登录,并且给予admin的Policy")]
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            return base.HandlerBuilder(builder);
        }

    }

    /// <summary>
    /// 退出登录
    /// </summary>
    [QuickApi("loginout", Group = "admin")]
    public class LoginOut : BaseQuickApiWithoutRequest<ContentResponse>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoginOut(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            _httpContextAccessor.HttpContext.SignOutAsync();
            return Task.FromResult(new ContentResponse("已经退出登录"));
        }

        [OpenApiOperation("loginout", "退出登录")]
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            return base.HandlerBuilder(builder);
        }

    }

    //测试权限组
    public abstract class BaseAdminApi<Req, Rsp> : BaseQuickApi<Req, Rsp> where Req : BaseRequest<Req>, new() where Rsp : BaseResponse
    {
        public override Task<Rsp> ExecuteAsync(Req request)
        {
            throw new NotImplementedException();
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //需要Admin的Policy才能访问
            builder.RequireAuthorization("admin");
            return base.HandlerBuilder(builder);
        }
    }

    /// <summary>
    /// 基本的权限测试
    /// </summary>
    [QuickApi("index", Group = "admin", Verbs = Verb.GET, Policy = "admin")]
    public class NeedAuthApi : BaseQuickApiWithoutRequest<ContentResponse>
    {
        public override Task<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(new ContentResponse("恭喜你有权限访问当前接口!"));
        }

        [OpenApiOperation("需要登录,NeedAuthApi", "NeedAuthApi")]
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //builder.WithOpenApi(operation => new(operation)
            //{
            //    Summary = "NeedAuthApi",
            //    Description = "NeedAuthApi"
            //});

            return base.HandlerBuilder(builder);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    [QuickApi("edit", Group = "admin", Verbs = Verb.GET)]
    public class EditDocumentApi : BaseAdminApi<EmptyRequest, IResultResponse>
    {
        public override Task<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(Results.Ok($"你有权限编辑!{DateTime.Now.ToLongTimeString()}").AsRsp());
        }

        [OpenApiOperation("需要登录,EditDocumentApi", "EditDocumentApi")]
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //cache
            builder.CacheOutput();
            return base.HandlerBuilder(builder);
        }
    }

    #endregion
}

#pragma warning restore