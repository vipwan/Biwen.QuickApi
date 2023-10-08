namespace Biwen.QuickApi
{
    /// <summary>
    /// 验证器接口
    /// </summary>
    internal interface IRequestValidator<T> where T : class, new()
    {
        IValidator<T> RealValidator { get; }
    }

}
