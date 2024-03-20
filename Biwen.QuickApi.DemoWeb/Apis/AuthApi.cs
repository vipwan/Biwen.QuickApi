using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Biwen.QuickApi.DemoWeb.Apis
{

    /// <summary>
    ///  模拟直接登录,并且给予admin的Policy
    /// </summary>
    [QuickApi("logined", Group = "admin")]
    [QuickApiSummary("模拟直接登录,并且给予admin的Policy", description: "模拟直接登录,并且给予admin的Policy")]
    public class Login : BaseQuickApiWithoutRequest<ContentResponse>
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        public Login(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            //模拟当前账号登录
            _httpContextAccessor.HttpContext?.SignInAsync(
                new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123456"),
                new Claim(ClaimTypes.Name, "vipwan@ms.co.ltd"),
                new Claim("admin", "admin"),
                new Claim(ClaimTypes.Role, "admin"),
            }, "admin")));

            return new ValueTask<ContentResponse>(new ContentResponse("已经登录成功"));

            //.net8当前支持 MapIdentityApi
            // ~/account/login 得到token

        }
    }

    /// <summary>
    /// 退出登录
    /// </summary>
    [QuickApi("loginout", Group = "admin")]
    [QuickApiSummary("退出登录", "退出登录")]
    public class LoginOut : BaseQuickApiWithoutRequest<ContentResponse>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoginOut(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            _httpContextAccessor.HttpContext?.SignOutAsync();
            return new ValueTask<ContentResponse>(new ContentResponse("已经退出登录"));
        }
    }

    //测试权限组
    public abstract class BaseAdminApi<Req, Rsp> : BaseQuickApi<Req, Rsp> where Req : BaseRequest<Req>, new() where Rsp : BaseResponse
    {
        public override ValueTask<Rsp> ExecuteAsync(Req request)
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
    [QuickApiSummary("需要登录,NeedAuthApi", "NeedAuthApi")]
    public class NeedAuthApi : BaseQuickApiWithoutRequest<ContentResponse>
    {
        public override ValueTask<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            return new ValueTask<ContentResponse>(new ContentResponse("恭喜你有权限访问当前接口!"));
        }

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
    [QuickApiSummary("需要登录,EditDocumentApi", description: "需要登录,EditDocumentApi")]
    public class EditDocumentApi : BaseAdminApi<EmptyRequest, IResultResponse>
    {
        public override ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            return new ValueTask<IResultResponse>(Results.Ok($"你有权限编辑!{DateTime.Now.ToLongTimeString()}").AsRspOfResult());
        }
    }


    [Authorize]
    [Authorize(policy: "admin")]
    [QuickApi("an-auth", Group = "admin")]
    [QuickApiSummary("使用特性标记需要登录", "使用特性标记需要登录")]
    public class AuthorizationTestApi : BaseQuickApi
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return Results.Content("登录成功的请求!").AsRspOfResult();
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithGroupName("admin");
            return base.HandlerBuilder(builder);
        }

    }

    [AllowAnonymous]
    [QuickApi("an-anonymous")]
    [QuickApiSummary("description: 使用特性标记可以匿名", "使用特性标记可以匿名")]
    public class AllowAnonymousTestApi : BaseQuickApi
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return Results.Content("无效登录的请求!").AsRspOfResult();
        }
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithGroupName("admin");
            return base.HandlerBuilder(builder);
        }
    }
}