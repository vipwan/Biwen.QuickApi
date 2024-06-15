namespace Biwen.QuickApi.Infrastructure
{
    /// <summary>
    /// 不被自动扫描注入的类型,如果开发者主动注入该特性将无意义
    /// </summary>
    [SuppressType, AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class SuppressTypeAttribute : Attribute
    {
    }
}