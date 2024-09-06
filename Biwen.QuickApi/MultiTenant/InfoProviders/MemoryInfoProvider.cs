// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:48:36 MemoryInfoProvider.cs

using Biwen.QuickApi.MultiTenant.Abstractions;

namespace Biwen.QuickApi.MultiTenant.InfoProviders;

/// <summary>
/// 基于内存提供租户信息
/// </summary>
/// <typeparam name="TInfo"></typeparam>
internal class MemoryInfoProvider<TInfo>() : ITenantInfoProvider<TInfo>
    where TInfo : class, ITenantInfo
{
    internal static volatile IList<TInfo>? _infos;

    public Task<IList<TInfo>> GetAllAsync()
    {
        return Task.FromResult(_infos ?? []);
    }
}