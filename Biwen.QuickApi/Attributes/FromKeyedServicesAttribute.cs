using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Biwen.QuickApi
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// 标记绑定属性为FromKeyedServices
    /// </summary>
    [Obsolete("不再提供,如需要请直接构造器注入服务")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromKeyedServicesAttribute : Attribute, IBindingSourceMetadata
    {
        public FromKeyedServicesAttribute(string key)
        {
            Key = key;
        }
        public string Key { get; set; }

        public BindingSource? BindingSource => default;
    }

#endif
}
