

namespace Biwen.QuickApi.Attributes
{

    /// <summary>
    /// 针对属性的别名,
    /// 一般用于Rsp,如果需要用于Req,请自定义Binder或者修改传输对象属性为Alias
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class AliasAsAttribute : Attribute
    {
        public AliasAsAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}