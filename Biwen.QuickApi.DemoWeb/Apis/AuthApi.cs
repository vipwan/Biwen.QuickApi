using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Biwen.QuickApi.DemoWeb.Apis
{

    /// <summary>
    ///  模拟直接登录,并且给予admin的Policy
    /// </summary>
    [QuickApi("logined", Group = "admin")]
    [OpenApiMetadata("Login", description: "模拟直接登录,并且给予admin的Policy")]
    public class Login : BaseQuickApi
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;


        public Login(IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
        {
            //模拟当前账号登录
            var result = await _userManager.CreateAsync(new IdentityUser
            {
                Email = "viwan@sina.com",
                UserName = "vipwan@sina.com"
            }, "123456");

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync("vipwan@sina.com");
                var flag = await _userManager.CheckPasswordAsync(user!, "123456");

                //添加claim
                await _userManager.AddClaimAsync(user!, new Claim("admin", "admin"));

                if (flag)
                {
                    await _signInManager.SignInAsync(new IdentityUser
                    {
                        Email = "viwan@sina.com",
                        UserName = "vipwan@sina.com"
                    }, true);
                    return Results.Ok("已经登录成功");
                }
                return Results.BadRequest("登录失败");
            }
            else
            {
                return Results.BadRequest($"注册账号失败了:{result.Errors.First().Description}");
            }
            //.net8当前支持 MapIdentityApi
            // ~/account/login 得到token
        }


        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.Produces<string>(200);
            return base.HandlerBuilder(builder);
        }

    }

    /// <summary>
    /// 退出登录
    /// </summary>
    [QuickApi("Loginout", Group = "admin")]
    [OpenApiMetadata("loginout", "退出登录")]
    public class LoginOut : BaseQuickApiWithoutRequest<string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoginOut(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<string> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
        {
            _httpContextAccessor.HttpContext?.SignOutAsync();
            return new ValueTask<string>("已经退出登录");
        }
    }

    //测试权限组
    public abstract class BaseAdminApi<Req, Rsp> : BaseQuickApi<Req, Rsp> where Req : BaseRequest<Req>, new() where Rsp : class
    {
        public override ValueTask<Rsp> ExecuteAsync(Req request, CancellationToken cancellationToken)
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
    [OpenApiMetadata("NeedAuthApi", "需要登录,NeedAuthApi")]
    public class NeedAuthApi : BaseQuickApi
    {
        public override ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(Results.Ok("恭喜你有权限访问当前接口!"));
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
    [OpenApiMetadata("Edit,需要登录", description: "需要登录,EditDocumentApi")]
    public class EditDocumentApi : BaseAdminApi<EmptyRequest, IResult>
    {
        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Results.Ok($"你有权限编辑!{DateTime.Now.ToLongTimeString()}");
        }
    }


    [Authorize]
    [Authorize(policy: "admin")]
    [QuickApi("an-auth", Group = "admin")]
    [OpenApiMetadata("[Authorize]特性", "使用特性标记需要登录")]
    public class AuthorizationTestApi : BaseQuickApi
    {
        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Results.Content("登录成功的请求!");
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithGroupName("admin");
            return base.HandlerBuilder(builder);
        }

    }

    [AllowAnonymous]
    [QuickApi("an-anonymous")]
    [OpenApiMetadata("[AllowAnonymous]匿名", "使用特性标记可以匿名")]
    public class AllowAnonymousTestApi : BaseQuickApi
    {
        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Results.Content("可以匿名的访问!");
        }
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithGroupName("admin");
            return base.HandlerBuilder(builder);
        }
    }
}