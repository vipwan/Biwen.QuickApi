using Biwen.QuickApi.DemoWeb.MultiTenant;
using Biwen.QuickApi.MultiTenant;
using Biwen.QuickApi.MultiTenant.Abstractions;

namespace Biwen.QuickApi.DemoWeb;

public class MultiTenantModular : ModularBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        //添加自定义的租户信息提供者
        services.AddTenantInfoProvider<MyTenantInfoProvider, TenantInfo>();

        //添加基于路径的租户查找器
        services.AddBasePathTenantFinder<TenantInfo>();

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

        group2.MapGet("api", async (IHttpContextAccessor ctx) =>
        {
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

    }
}