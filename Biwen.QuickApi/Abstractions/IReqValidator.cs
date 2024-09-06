// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 IReqValidator.cs

namespace Biwen.QuickApi.Abstractions
{
    /// <summary>
    /// 验证器接口
    /// </summary>
#pragma warning disable GEN031 // 使用[AutoGen]自动生成
    internal interface IReqValidator<T> : IReqValidator where T : class, new()
    {
    }
#pragma warning restore GEN031 // 使用[AutoGen]自动生成

    internal interface IReqValidator
    {
        /// <summary>
        /// 验证当前的Request
        /// </summary>
        /// <returns></returns>
        ValidationResult Validate();
    }
}