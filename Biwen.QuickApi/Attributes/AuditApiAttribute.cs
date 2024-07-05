namespace Biwen.QuickApi.Attributes;

/// <summary>
/// 标记QuickApi或者QuickEndpoint需要审计
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AuditApiAttribute : Attribute
{
}