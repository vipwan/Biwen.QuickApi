﻿// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:01:27 Constant.cs

using System.Diagnostics;
using System.Reflection;

namespace Biwen.QuickApi.Telemetry;

public static class Constant
{
    /// <summary>
    /// 前缀
    /// </summary>
    internal const string Prefix = "biwen_quickapi_";

    /// <summary>
    /// The assembly.
    /// </summary>
    internal static readonly Assembly Assembly = typeof(TelemetryModular).Assembly;

    /// <summary>
    /// The assembly name.
    /// </summary>
    internal static readonly AssemblyName AssemblyName = Assembly.GetName();

    /// <summary>
    /// The activity source name.
    /// </summary>
    internal static readonly string OpenTelemetryActivitySourceName = AssemblyName.Name!;

    /// <summary>
    /// Version
    /// </summary>
    internal static readonly string OpenTelemetryVersion = Assembly.GetPackageVersion();


    /// <summary>
    /// CPU 使用比例 %
    /// </summary>
    public const string CpuUsedPercentage = "CpuUsedPercentage";

    /// <summary>
    /// 内存使用比例 %
    /// </summary>
    public const string MemoryUsedPercentage = "MemoryUsedPercentage";

    /// <summary>
    /// 内存已使用量 bytes
    /// </summary>
    public const string MemoryUsedInBytes = "MemoryUsedInBytes";

    /// <summary>
    /// 内存总计 bytes
    /// </summary>
    public const string MaximumMemoryInBytes = "MaximumMemoryInBytes";



    /// <summary>
    /// 获取包版本
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static string GetPackageVersion(this Assembly assembly)
    {
        // MinVer https://github.com/adamralph/minver?tab=readme-ov-file#version-numbers
        // together with Microsoft.SourceLink.GitHub https://github.com/dotnet/sourcelink
        // fills AssemblyInformationalVersionAttribute by
        // {majorVersion}.{minorVersion}.{patchVersion}.{pre-release label}.{pre-release version}.{gitHeight}+{Git SHA of current commit}
        // Ex: 1.5.0-alpha.1.40+807f703e1b4d9874a92bd86d9f2d4ebe5b5d52e4
        // The following parts are optional: pre-release label, pre-release version, git height, Git SHA of current commit
        // For package version, value of AssemblyInformationalVersionAttribute without commit hash is returned.

        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        Debug.Assert(!string.IsNullOrEmpty(informationalVersion), "AssemblyInformationalVersionAttribute was not found in assembly");

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        var indexOfPlusSign = informationalVersion!.IndexOf('+', StringComparison.Ordinal);
#else
    var indexOfPlusSign = informationalVersion!.IndexOf('+');
#endif
        return indexOfPlusSign > 0
            ? informationalVersion.Substring(0, indexOfPlusSign)
            : informationalVersion;
    }

}