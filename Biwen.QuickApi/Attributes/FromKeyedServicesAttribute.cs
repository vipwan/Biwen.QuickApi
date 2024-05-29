namespace Biwen.QuickApi
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// 标记绑定属性为FromKeyedServices
    /// </summary>
    [Obsolete("不再提供,如需要注入服务,请直接在构造器中注入即可!")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromKeyedServicesAttribute : Attribute
    {
        public FromKeyedServicesAttribute(string key)
        {
            Key = key;
        }
        public string Key { get; set; }
    }

#endif
}
