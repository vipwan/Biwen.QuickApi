// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:49:16 TenantInfo.cs

namespace Biwen.QuickApi.MultiTenant;

/// <summary>
/// 默认的租户信息
/// </summary>
public record class TenantInfo : ITenantInfo
{
    private string id = null!;

    public string Id
    {
        get
        {
            return id;
        }
        set
        {
            if (value != null)
            {
                if (value.Length > 256)
                {
                    throw new QuickApiExcetion($"The tenant id cannot exceed {256} characters.");
                }
                id = value;
            }
        }
    }

    public required string Identifier { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// 链接字符串
    /// </summary>
    public string? ConnectionString { get; set; }
}