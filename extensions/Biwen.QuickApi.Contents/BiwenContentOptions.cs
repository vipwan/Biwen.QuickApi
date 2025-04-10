// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-05 22:27:51 BiwenContentOptions.cs


using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Contents;

public class BiwenContentOptions
{

    /// <summary>
    /// 是否启用Api接口,默认:true
    /// </summary>
    public bool EnableApi { get; set; } = true;

    /// <summary>
    /// 是否启用OpenApi文档生成,默认:true
    /// </summary>
    public bool GenerateApiDocument { get; set; } = true;


    /// <summary>
    /// 权限验证器,如果返回false则不允许访问设置页面,默认:允许任意访问
    /// </summary>
    public Func<HttpContext, Task<bool>> PermissionValidator { get; set; } = _ => Task.FromResult(true);

    /// <summary>
    /// 默认文档视图存放的路径,默认:"\\Views\\Contents",约定使用双反斜杠定位路径
    /// </summary>
    public string? ViewPath { get; set; } = "\\Views\\Contents";


    /// <summary>
    /// Minimal Api前缀,默认:""
    /// </summary>
    ///public string ApiPrefix { get; set; } = string.Empty;

}
