namespace Biwen.QuickApi
{
    /// <summary>
    /// 验证器接口
    /// </summary>
    internal interface IReqValidator<T> where T : class, new()
    {
        IValidator<T> RealValidator { get; }
        /// <summary>
        /// 验证当前的Request
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        ValidationResult Validate();

    }

}
