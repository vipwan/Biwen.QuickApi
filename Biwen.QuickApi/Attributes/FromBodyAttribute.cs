using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Biwen.QuickApi
{
    /// <summary>
    /// 标记整个Request对象为FromBody
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class FromBodyAttribute : Attribute, IBindingSourceMetadata
    {
        public BindingSource? BindingSource => default;
    }
}
