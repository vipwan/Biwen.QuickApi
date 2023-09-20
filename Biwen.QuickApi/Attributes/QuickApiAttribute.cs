
namespace Biwen.QuickApi.Attributes
{

    /// <summary>
    /// QuickApi特性
    /// </summary>

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class QuickApiAttribute : Attribute
    {
        public QuickApiAttribute(string route)
        {
            Route = route;
        }
        /// <summary>
        /// 分组. 例如: hello
        /// </summary>
        public string Group { get; set; } = string.Empty;
        /// <summary>
        /// 路由. 例如: hello/world/{name}
        /// </summary>
        public string Route { get; set; } = null!;
        /// <summary>
        /// 请求方式. 默认为GET, 如果需要多种方式，可以使用 | 运算符
        /// </summary>
        public Verb Verbs { get; set; } = Verb.GET;
        /// <summary>
        /// 请求策略. 默认为null，表示不需要验证
        /// </summary>
        public string? Policy { get; set; }
    }

    /// <summary>
    /// 请求参数来源
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class FromAttribute : Attribute
    {
        public FromAttribute(RequestFrom from)
        {
            From = from;
        }
        public RequestFrom From { get; set; }
    }
}