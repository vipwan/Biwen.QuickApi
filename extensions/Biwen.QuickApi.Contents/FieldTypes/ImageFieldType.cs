// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 00:12:27 ImageFieldType.cs

namespace Biwen.QuickApi.Contents.FieldTypes;

/// <summary>
/// 图片字段实现.和TextFieldType基本重叠
/// </summary>
public class ImageFieldType : TextFieldType
{
    public override string Name => "图片";
    public override string SystemName => "imageInput";

    // 添加带参构造函数
    public ImageFieldType() { }
    public ImageFieldType(string value) : base(value)
    {
        Value = value;
    }
}