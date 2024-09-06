// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:48:41 MultiTenantMiddleware.cs

using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Internal;

/// <summary>
/// 附加租户信息的中间件
/// </summary>
/// <typeparam name="TInfo"></typeparam>
internal class MultiTenantMiddleware<TInfo> where TInfo : class, ITenantInfo
{
    private readonly RequestDelegate _next;
    private readonly IOptions<MultiTenantOptions> _options;
    private readonly Lazy<ITenantInfoProvider<TInfo>> _infoProvider;
    private readonly Lazy<ITenantFinder<TInfo>> _finder;
    private readonly Lazy<AsyncContextService<TInfo>> _contextService;

    /// <summary>
    /// 缓存默认的租户信息
    /// </summary>
    private static TInfo CachedDefaultTenantInfo = null!;

    public MultiTenantMiddleware(RequestDelegate next, IOptions<MultiTenantOptions> options, IHttpContextAccessor httpContextAccessor)
    {
        _next = next;
        _options = options;

        _infoProvider = new Lazy<ITenantInfoProvider<TInfo>>(() =>
        httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<CachingProxyFactory<ITenantInfoProvider<TInfo>>>().Create());

        _finder = new Lazy<ITenantFinder<TInfo>>(() =>
        httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<ITenantFinder<TInfo>>());

        _contextService = new Lazy<AsyncContextService<TInfo>>(() =>
        httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<AsyncContextService<TInfo>>());
    }

    public async Task Invoke(HttpContext context)
    {
        if (!_options.Value.Enabled)
        {
            await _next(context);
            return;
        }
        if (_contextService.Value.TryGet(out var info) && info is not null)
        {
            await _next(context);
            return;
        }

        var tenantInfo = await _finder.Value.FindAsync();
        if (tenantInfo is not null)
        {
            _contextService.Value.Set(tenantInfo);
        }
        else
        {
            //当没有找到租户信息时
            if (!string.IsNullOrWhiteSpace(_options.Value.DefaultId))
            {
                if (CachedDefaultTenantInfo is null)
                {
                    var all = await _infoProvider.Value.GetAllAsync();
                    //如果配置的默认Id不存在,则抛出异常!
                    var defaultInfo = all.FirstOrDefault(t =>
                    t.Id.Equals(_options.Value.DefaultId, StringComparison.OrdinalIgnoreCase));
                    if (defaultInfo is null)
                    {
                        throw new QuickApiExcetion($"默认的租户信息不存在,Id:{_options.Value.DefaultId} !");
                    }
                    CachedDefaultTenantInfo = defaultInfo;
                }
                _contextService.Value.Set(CachedDefaultTenantInfo);
            }
        }
        await _next(context);
    }
}