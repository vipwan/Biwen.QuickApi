﻿using Biwen.QuickApi.DemoWeb.Apis.Endpoints;
using Biwen.QuickApi.DemoWeb.Components;
using Biwen.QuickApi.DemoWeb.Schedules;
using Biwen.QuickApi.FeatureManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using Constants = Biwen.QuickApi.FeatureManagement.Constants;

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


            services.ConfigureQuickApiFeatureManagementOptions(o =>
            {
                //自定义特性管理模块的返回状态码
                o.StatusCode = StatusCodes.Status405MethodNotAllowed;
                //自定义特性管理模块的错误处理
                //o.OnErrorAsync = ctx =>
                //{
                //    ctx.Response.WriteAsync(o.StatusCode.ToString());
                //};
            });

            //在前置模块配置DemoOptions,DemoModular就中可以直接注入
            services.AddOptions<DemoOptions>().Configure(c =>
            {
                c.Name += "!!!!";
                c.Enable = true;
            });

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
    public class DemoModular(/*IHostEnvironment environment, */IOptions<DemoOptions> options) : ModularBase
    {
        public override int Order => Constants.Order + 1;

        /// <summary>
        /// 请注意,如果需要在模块中使用配置,请使用构造函数注入,且需要在前置模块中配置!
        /// 当然你也可以直接注入IConfiguration获取.
        /// </summary>
        public DemoOptions Options { get; } = options.Value;

        /// <summary>
        /// 测试模块仅用于开发测试
        /// </summary>
        public override Func<bool> IsEnable => () => options.Value.Enable; //environment.IsDevelopment();


        public override void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
            //services.AddScoped<HelloService>();

            services.AddAutoInject();


            // keyed services
            //builder.Services.AddKeyedScoped<HelloService>("hello");
        }

        /// <summary>
        /// 模拟的缓存数据类型
        /// </summary>
        /// <param name="DateTime"></param>
        public record CacheData(DateTime? DateTime);


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

                x.MapGet("/outputcache/{id:int?}", (int? id) =>
                {
                    return Results.Content($"{id}-{DateTime.Now}");
                }).CacheOutput(policy =>
                {
                    //缓存10s过期
                    policy.Expire(TimeSpan.FromSeconds(10d));
                });

                //分布式缓存,性能不佳,建议使用.NET9新增hybrid缓存
                x.MapGet("/cached-in-distribute", async (IDistributedCache distributedCache) =>
                {
                    if (await distributedCache.GetStringAsync("$cached-in-memory") is null)
                    {
                        var data = System.Text.Json.JsonSerializer.Serialize(new CacheData(DateTime.Now));
                        await distributedCache.SetStringAsync("$cached-in-memory", data, new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10d)
                        });
                    }

                    var fromCacheData = System.Text.Json.JsonSerializer.Deserialize<CacheData>(
                        await distributedCache.GetStringAsync("$cached-in-memory") ?? throw new Exception());

                    return Results.Content($"{fromCacheData?.DateTime}-{DateTime.Now}");
                }).WithDescription("分布式缓存,存在序列化&反序列化,性能较差");

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
                //~/endpoints/hello/blazor-render-svc
                x.MapMethods<BlazorRenderSvcEndpoint>("hello/blazor-render-svc");

                //Feature测试
                x.MapMethods<FeatureTestEndpoint>("hello/feature-test");


            });

            //测试特性管理
            routes.MapGet(
                "/new-feature",
                () => Results.Content("new feature!"))
                .WithMetadata(new FeatureGateAttribute("myfeature"));

            // Identity API {"email" : "vipwan@co.ltd","password" : "*******"}
            // ~/account/register    
            // ~/account/login 

            //当前preview4 BUG因此必须:ExcludeFromDescription()
            routes.MapGroup("account").MapIdentityApi<IdentityUser>();

            routes.MapGet("/hello-demo", (IOptions<DemoOptions> options) => options.Value.Name);
        }

    }



    public class DemoOptions
    {
        public string Name { get; set; } = "Demo";

        public bool Enable { get; set; } = true;
    }

}