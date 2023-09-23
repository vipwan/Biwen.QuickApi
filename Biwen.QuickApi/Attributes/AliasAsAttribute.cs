

namespace Biwen.QuickApi.Attributes
{

    /// <summary>
    /// 针对属性的别名,
    /// 如果是自定义ReqBinder,需要自行处理
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