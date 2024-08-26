using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Internal;

internal class MultiTenantMiddleware<TInfo> where TInfo : class, ITenantInfo
{
    private readonly RequestDelegate next;

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

        await next(context);
    }
}
