using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Internal;

internal class MultiTenantMiddleware<TInfo> where TInfo : class, ITenantInfo
{
    private readonly RequestDelegate next;

    /// <summary>
    /// 缓存默认的租户信息
    /// </summary>
    private static TInfo CachedDefaultTenantInfo = null!;

    public MultiTenantMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<MultiTenantOptions>>();
        if (!options.Value.Enabled)
        {
            await next(context);
            return;
        }

        var tenantContextService = context.RequestServices.GetRequiredService<AsyncContextService<TInfo>>();
        if (tenantContextService.TryGet(out var info) && info is not null)
        {
            await next(context);
            return;
        }

        var tenantFinder = context.RequestServices.GetRequiredService<ITenantFinder<TInfo>>();
        var tenantInfo = await tenantFinder.FindAsync();

        if (tenantInfo is not null)
        {
            tenantContextService.Set(tenantInfo);
        }
        else
        {
            //当没有找到租户信息时
            if (!string.IsNullOrWhiteSpace(options.Value.DefaultId))
            {
                if (CachedDefaultTenantInfo is null)
                {
                    var infoProvider = context.RequestServices.GetRequiredService<ITenantInfoProvider<TInfo>>();
                    var all = await infoProvider.GetAll();
                    //如果配置的默认Id不存在,则抛出异常!
                    var defaultInfo = all.FirstOrDefault(t => t.Id == options.Value.DefaultId);

                    if (defaultInfo is null)
                    {
                        throw new QuickApiExcetion($"默认的租户信息不存在,Id:{options.Value.DefaultId} !");
                    }

                    CachedDefaultTenantInfo = defaultInfo;
                }

                tenantContextService.Set(CachedDefaultTenantInfo);
            }
        }

        await next(context);
    }
}