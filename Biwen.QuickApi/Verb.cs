// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:08:54 Verb.cs

namespace Biwen.QuickApi
{
    /// <summary>
    ///  请求方式
    /// </summary>
    [Flags]
    public enum Verb
    {
        /// <summary>
        /// 这是默认值，如果没有指定Verb，那么默认是GET
        /// </summary>
        GET = 1,
        /// <summary>
        /// POST
        /// </summary>
        POST = 2,
        /// <summary>
        /// PUT
        /// </summary>
        PUT = 4,
        DELETE = 8,
        /// <summary>
        /// PATCH
        /// </summary>
        PATCH = 16,
        /// <summary>
        /// HEAD
        /// </summary>
        HEAD = 32,
        /// <summary>
        /// OPTIONS
        /// </summary>
        OPTIONS = 64,
    }
}
