// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 AuditInfo.cs

using Biwen.QuickApi.Serializer;

namespace Biwen.QuickApi.Auditing;

[Serializable]
public class AuditInfo
{
    public string? ApplicationName { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? ClientIpAddress { get; set; }

    public string? ClientName { get; set; }

    public string? BrowserInfo { get; set; }

    public string? HttpMethod { get; set; }

    public string? Url { get; set; }

    public ActionInfo? ActionInfo { get; set; }

    /// <summary>
    /// 是否是QuickApi或者QuickEndpoint
    /// </summary>
    public bool IsQuickApi { get; set; }


    /// <summary>
    /// 扩展信息
    /// </summary>
    public Dictionary<string, object?>? ExtraInfos { get; set; }

    /// <summary>
    /// JSON序列化
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return DefaultSerializer.Instance.SerializeToString(this)!;
    }

}

[Serializable]
public class ActionInfo
{
    public MethodInfo? MethodInfo { get; set; }

}
