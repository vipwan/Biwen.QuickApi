namespace Biwen.QuickApi.Abstractions
{
    /// <summary>
    /// 验证器接口
    /// </summary>
    internal interface IReqValidator<T> where T : class, new()
    {
        /// <summary>
        /// 验证当前的Request
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        ValidationResult Validate();

    }

}
