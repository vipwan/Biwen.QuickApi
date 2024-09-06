// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:48:02 ITenantInfoProvider.cs

namespace Biwen.QuickApi.MultiTenant.Abstractions;

/// <summary>
/// 租户信息提供者
/// </summary>
/// <typeparam name="TInfo"></typeparam>
public interface ITenantInfoProvider<TInfo> where TInfo : ITenantInfo
{
    /// <summary>
    /// 请注意如果是通过数据库,配置文件等方式,内部请使用缓存提升性能
    /// </summary>
    /// <returns></returns>
    [AutoCache]
    Task<IList<TInfo>> GetAllAsync();
}