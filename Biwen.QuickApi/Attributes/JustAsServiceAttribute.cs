
namespace Biwen.QuickApi.Attributes
{


    /// <summary>
    /// 标记QuickApi只在服务中使用,不注册Api路由
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class JustAsServiceAttribute : Attribute
    {
    }
}