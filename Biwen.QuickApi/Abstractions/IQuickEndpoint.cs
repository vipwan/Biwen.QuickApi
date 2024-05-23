namespace Biwen.QuickApi.Abstractions
{
    /// <summary>
    /// 对Minimal Api的扩展
    /// </summary>
    public interface IQuickEndpoint
    {
        /// <summary>
        /// 请求方式 支持多个,如: Verb.GET | Verb.POST,默认:Verb.GET
        /// </summary>
        /// <returns></returns>
        public static abstract Verb Verbs { get; }
        /// <summary>
        /// MinimalApi执行的Handler
        /// </summary>
        /// <returns></returns>
        public static abstract Delegate Handler { get; }

    }
}
