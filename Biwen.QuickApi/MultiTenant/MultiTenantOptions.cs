// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:49:01 MultiTenantOptions.cs

namespace Biwen.QuickApi.MultiTenant;

public class MultiTenantOptions
{
    public bool Enabled { get; internal set; } = true;

    public Type? TenantInfoType { get; internal set; }

    /// <summary>
    /// 默认的租户标识符,如果没有找到租户信息,则使用Id,
    /// 默认为null表示不使用,表示如果没有找到租户信息,则不进行处理
    /// </summary>
    public string? DefaultId { get; set; } = null;

}
