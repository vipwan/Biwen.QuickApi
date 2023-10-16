namespace Biwen.QuickApi
{
    /// <summary>
    /// 验证器接口
    /// </summary>
    internal interface IReqValidator<T> where T : class, new()
    {
        [Obsolete("请使用Validate(),RealValidator不支持DataAnnotations", false)]
        IValidator<T> RealValidator { get; }
        /// <summary>
        /// 验证当前的Request
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        ValidationResult Validate();

    }

}
