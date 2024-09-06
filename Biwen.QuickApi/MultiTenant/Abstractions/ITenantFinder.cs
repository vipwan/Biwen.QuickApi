// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:47:58 ITenantFinder.cs

namespace Biwen.QuickApi.MultiTenant.Abstractions;

/// <summary>
/// 租户查找器
/// </summary>
public interface ITenantFinder<TInfo> where TInfo : ITenantInfo
{
    /// <summary>
    /// 查找租户信息,如果找不到返回null
    /// </summary>
    /// <returns></returns>
    Task<TInfo?> FindAsync();
}