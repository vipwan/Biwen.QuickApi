namespace Biwen.QuickApi.Abstractions.Modular
{
    /// <summary>
    /// 前置模块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class PreModularAttribute<T> : Attribute where T : ModularBase
    {
        /// <summary>
        /// 前置模块类型
        /// </summary>
        public Type PreModular { get; set; } = typeof(T);
    }
}