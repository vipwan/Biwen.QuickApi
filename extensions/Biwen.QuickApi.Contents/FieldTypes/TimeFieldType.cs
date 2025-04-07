// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 04:05:15 TimeFieldType.cs



namespace Biwen.QuickApi.Contents.FieldTypes;

/// <summary>
/// HH:mm:ss
/// </summary>
public class TimeFieldType : TextFieldType
{
    public override string Name => "时间";
    public override string SystemName => "timePicker";

}