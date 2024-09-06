// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 AuditIgnoreAttribute.cs

namespace Biwen.QuickApi.Auditing;

/// <summary>
/// 忽略审计
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AuditIgnoreAttribute : Attribute
{
    /// <summary>
    /// 忽略类型
    /// </summary>
    public IgnoreType IgnoreType { get; set; } = IgnoreType.All;

    /// <summary>
    /// 忽略审计
    /// </summary>
    /// <param name="ignoreType"></param>
    public AuditIgnoreAttribute(IgnoreType ignoreType = IgnoreType.All)
    {
        IgnoreType = ignoreType;
    }
}

[Flags]
public enum IgnoreType
{
    /// <summary>
    /// 默认完全忽略
    /// </summary>
    All = 1,
    /// <summary>
    /// 忽略传参
    /// </summary>
    Parameter = 2,
    /// <summary>
    /// 忽略返回值
    /// </summary>
    ReturnValue = 4,
}