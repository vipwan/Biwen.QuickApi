// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:48:54 ITenantInfo.cs

namespace Biwen.QuickApi.MultiTenant;

/// <summary>
/// 多租户信息
/// </summary>
public interface ITenantInfo
{
    /// <summary>
    /// 租户标识Id忽略大小写
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// 用于鉴别的标识,对于域名等方式的租户鉴别,使用正则表达式
    /// </summary>
    string Identifier { get; set; }

    /// <summary>
    /// 别名
    /// </summary>
    string Name { get; }

}
