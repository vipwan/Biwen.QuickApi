// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:46:44 Assemblies.cs

using Biwen.QuickApi.Infrastructure.TypeFinder;

namespace Biwen.QuickApi.Infrastructure;


/// <summary>
/// Assembly Helper
/// </summary>
[SuppressType]
internal static class Assemblies
{
    /// <summary>
    /// 排除的程序集
    /// </summary>
    private static readonly string[] EscapeAssemblies =
    {
        "netstandard",
        "Microsoft",
        "Mono",
        "AngleSharp",//AngleSharp
        "Mapster",//Mapster
        "Scrutor",//Scrutor
        "Humanizer",
        "SQLitePCLRaw",//Sqlite
        "System",
        "Newtonsoft",
        "Swashbuckle",
        "AutoMapper",
        "FluentValidation",
        "Asp.Versioning",
        "NSwag",
        "YamlDotNet",
        "NCrontab",
        "NJsonSchema",
        "Namotion.Reflection",
        "MiniProfiler",
        "Serilog",//Serilog
        "OpenTelemetry",//OpenTelemetry
        "Biwen.AutoClassGen" //AutoClassGen
    };

    private static Assembly[] _allRequiredAssemblies = null!;
    private static bool _allRequiredAssembliesFound = false;

    /// <summary>
    /// 排除公共程序集后的所有程序集
    /// </summary>
    public static Assembly[] AllRequiredAssemblies
    {
        get
        {
            if (!_allRequiredAssembliesFound)
            {
                // 装载所有引用的程序集
                //var ass = Assembly.GetEntryAssembly()!.GetReferencedAssemblies();
                //foreach (var @as in ass)
                //{
                //    Assembly.Load(@as);
                //}

                var dlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
                //排除程序集名称以EscapeAssemblies集合开头的程序集:
                var needDlls = new List<string>();
                foreach (var dll in dlls)
                {
                    var assemblyName = Path.GetFileName(dll);
                    var flag = false;
                    foreach (var escape in EscapeAssemblies)
                    {
                        if (assemblyName.StartsWith(escape))
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                        needDlls.Add(dll);
                }

                var assemblies = needDlls.Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)));
                _allRequiredAssemblies ??= [.. assemblies];
                _allRequiredAssembliesFound = true;
            }
            return _allRequiredAssemblies;
        }
    }

    /// <summary>
    /// Extension
    /// </summary>
    public static IInAssemblyFinder InAllRequiredAssemblies => FindTypes.InAssemblies(AllRequiredAssemblies);

}