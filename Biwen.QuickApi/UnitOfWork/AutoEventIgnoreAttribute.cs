namespace Biwen.QuickApi.UnitOfWork;

/// <summary>
/// 标注的Entity类型将不会自动广播增删改事件
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AutoEventIgnoreAttribute : Attribute
{
}
