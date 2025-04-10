// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-10 16:15:35 ContentViewModel.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Domain;

namespace Biwen.QuickApi.Contents.Rendering;

public class ContentViewModel<T> where T : IContent
{

    /// <summary>
    /// 文档
    /// </summary>
    public T Content { get; set; } = default!;

    /// <summary>
    /// 文档定义数据
    /// </summary>
    public Content ContentDefine { get; set; } = default!;

}
