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