// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:09:13 QuickApiExcetion.cs

namespace Biwen.QuickApi;

/// <summary>
/// QuickApi异常
/// </summary>
public class QuickApiExcetion(string message) : Exception(message)
{
}