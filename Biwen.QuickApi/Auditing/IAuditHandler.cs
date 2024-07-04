namespace Biwen.QuickApi.Auditing;

public interface IAuditHandler
{
    /// <summary>
    /// 处理审计日志
    /// </summary>
    /// <param name="auditInfo"></param>
    Task Handle(AuditInfo auditInfo);

}
