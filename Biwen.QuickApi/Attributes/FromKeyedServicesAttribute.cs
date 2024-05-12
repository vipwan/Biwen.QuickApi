namespace Biwen.QuickApi
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// 标记绑定属性为FromKeyedServices
    /// </summary>
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
