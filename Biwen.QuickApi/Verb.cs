﻿namespace Biwen.QuickApi
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
        POST = 2,
        PUT = 4,
        DELETE = 8,
        PATCH = 16,
        HEAD = 32,
        OPTIONS = 64,
    }
}
