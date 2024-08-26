using Biwen.QuickApi.DemoWeb.MultiTenant;
using Biwen.QuickApi.MultiTenant;
using Biwen.QuickApi.MultiTenant.Abstractions;

namespace Biwen.QuickApi.DemoWeb;

public class MultiTenantModular : ModularBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        ////添加自定义的租户信息提供者
        //services.AddTenantInfoProvider<MyTenantInfoProvider, TenantInfo>();

        ////添加基于路径的租户查找器
        //services.AddBasePathTenantFinder<TenantInfo>();

        services.AddMultiTenant<TenantInfo>()
            .AddTenantInfoProvider<MyTenantInfoProvider>()
            .AddBasePathTenantFinder();

        //添加基于Header的租户查找器
        //services.AddHeaderTenantFinder<TenantInfo>("X-Tenant-Id");

        //请注意系统只支持一种租户查找器.后注册的会覆盖前面的
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        //添加租户1的路由
        var group1 = routes.MapGroup("tenant1");

        group1.MapGet("api", async (IHttpContextAccessor ctx) =>
        {
            //方式一使用ITenantFinder获取租户信息,不推荐使用此方式,
            //但是当没有注册`UseMultiTenant<T>`时或者注册中间件顺序问题,只能使用此方式!
            var tenantFinder = ctx.HttpContext!.RequestServices.GetRequiredService<ITenantFinder<TenantInfo>>();
            var tenantInfo = await tenantFinder.FindAsync();

            if (tenantInfo == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(tenantInfo);
        })
        .WithTags(["MutiTenant"])
        .Produces(StatusCodes.Status404NotFound)
        .Produces<List<TenantInfo>>();

        //添加租户2的路由
        var group2 = routes.MapGroup("tenant2");

        group2.MapGet("api", (IHttpContextAccessor ctx) =>
        {
            //var tenantFinder = ctx.HttpContext!.RequestServices.GetRequiredService<ITenantFinder<TenantInfo>>();
            //var tenantInfo = await tenantFinder.FindAsync();

            //方式二:使用扩展方法获取租户信息,推荐使用此方式
            var tenantInfo = ctx.HttpContext!.GetTenantInfo<TenantInfo>();

            if (tenantInfo == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(tenantInfo);
        })
        .WithTags(["MutiTenant"])
        .Produces(StatusCodes.Status404NotFound)
        .Produces<List<TenantInfo>>();

    }
}