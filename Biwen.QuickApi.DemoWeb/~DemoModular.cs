using Biwen.QuickApi.DemoWeb.Apis.Endpoints;
using Biwen.QuickApi.DemoWeb.Components;
using Biwen.QuickApi.DemoWeb.Schedules;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;

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
            //hybrid cache NET9 新功能,多级缓存避免分布式缓存的强制转换,需要引用Microsoft.Extensions.Caching.Hybri
            services.AddHybridCache(options =>
            {

                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(5 * 60),//默认5分钟缓存
                    LocalCacheExpiration = TimeSpan.FromSeconds(5 * 60 - 1)//本地缓存提前1秒过期

                };
            });

            // Add services to the container.
            services.AddScoped<HelloService>();
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

                //hybrid缓存,避免分布式缓存的强制转换
                x.MapGet("/cached-in-hybrid", async (HybridCache hybridCache) =>
                {
                    var cachedDate = await hybridCache.GetOrCreateAsync($"$cached-in-hybrid", async cancel =>
                      {
                          return await ValueTask.FromResult(new CacheData(DateTime.Now));
                      }, options: new HybridCacheEntryOptions
                      {
                          Expiration = TimeSpan.FromSeconds(10d),//便于验证,设直10秒过期
                          LocalCacheExpiration = TimeSpan.FromSeconds(10d),
                      });
                    return Results.Content($"缓存的数据:{cachedDate.DateTime}");
                }).WithDescription("多级缓存,避免分布式缓存的频繁序列化反序列化");

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