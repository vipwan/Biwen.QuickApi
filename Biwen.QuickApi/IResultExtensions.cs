// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:09:28 IResultExtensions.cs

using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi
{
    [SuppressType]
    public static class IResultExtensions
    {
        /// <summary>
        /// 转换为IResultResponse
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
#pragma warning disable CS0618 // 类型或成员已过时
        public static IResultResponse AsRspOfResult(this IResult result)
        {
            return new IResultResponse(result ?? throw new ArgumentNullException(nameof(result)));
        }
    }
#pragma warning restore CS0618 // 类型或成员已过时

}