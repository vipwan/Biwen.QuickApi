// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:49:12 TenantContextAccessor.cs

using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant;

public sealed class TenantContextAccessor<TInfo>(IHttpContextAccessor httpContextAccessor)
    where TInfo : class, ITenantInfo
{

    private TInfo? _tenantInfo;

    /// <summary>
    /// 获取上下文中的租户信息
    /// </summary>
    public TInfo? TenantInfo
    {
        get
        {
            if (_tenantInfo is not null)
            {
                return _tenantInfo;
            }

            if (httpContextAccessor?.HttpContext is null)
            {
                throw new InvalidOperationException("HttpContext is null");
            }

            var contextService = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<AsyncContextService<TInfo>>();
            contextService.TryGet(out _tenantInfo);

            return _tenantInfo;
        }
    }
}