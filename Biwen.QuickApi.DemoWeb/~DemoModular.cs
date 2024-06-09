using Biwen.QuickApi.DemoWeb.Apis.Endpoints;
using Biwen.QuickApi.DemoWeb.Components;
using Biwen.QuickApi.DemoWeb.Schedules;
using Microsoft.AspNetCore.Identity;

namespace Biwen.QuickApi.DemoWeb
{
    /// <summary>
    /// 前置的模块
    /// </summary>
    public class PreModular1 : ModularBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Add ScheduleTaskStore
            services.AddScheduleMetadataStore<DemoStore>();
        }
        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            base.Configure(app, routes, serviceProvider);
        }
    }

    /// <summary>
    /// Demo模块
    /// </summary>
    /// <param name="environment"></param>
    [PreModular<PreModular1>]
    public class DemoModular(IHostEnvironment environment) : ModularBase
    {
        /// <summary>
        /// 测试模块仅用于开发测试
        /// </summary>
        public override Func<bool> IsEnable => () => environment.IsDevelopment();

        public override void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
            services.AddScoped<HelloService>();
            // keyed services
            //builder.Services.AddKeyedScoped<HelloService>("hello");
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapGroup("root", x =>
            {
                x.MapGet("/binder", (HttpContext context, BindRequest request) =>
                {
                    //测试默认绑定器
                    return Results.Content(request.Hello);
                });

                //测试HtmlSanitizer
                x.MapGet("/xss", () => { return "<a href=\"javascript: alert('xss')\">Click me</a>".SanitizeHtml(); });

                x.MapGet("/cached/{id:int?}", (int? id) =>
                {
                    return Results.Content($"{id}-{DateTime.Now}");
                }).CacheOutput(policy =>
                {
                    //缓存10s过期
                    policy.Expire(TimeSpan.FromSeconds(10d));
                });

                x.MapComponent<HelloWorld>("/razor/{key}",
                    context =>
                    {
                        return new { Key = context.Request.RouteValues["key"] };
                    });
            });


            //提供IQuickEndpoint支持:
            routes.MapGroup("endpoints", x =>
            {
                //~/endpoints/hello/hello?key=world
                x.MapMethods<HelloEndpoint>("hello/{hello}");
                x.MapMethods<PostDataEndpoint>("hello/postdata");
            });

            // Identity API {"email" : "vipwan@co.ltd","password" : "*******"}
            // ~/account/register    
            // ~/account/login 

            if (environment.IsDevelopment())
            {
                //当前preview4 BUG因此必须:ExcludeFromDescription()
                routes.MapGroup("account").MapIdentityApi<IdentityUser>().ExcludeFromDescription();
            }
            else
            {
                routes.MapGroup("account").MapIdentityApi<IdentityUser>();
            }
        }

    }
}