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
