namespace Biwen.QuickApi.Auditing;

internal class ConsoleAuditHandler(ILogger<ConsoleAuditHandler> logger) : IAuditHandler
{
    public Task Handle(AuditInfo auditInfo)
    {
        logger.LogInformation("AuditInfo: {@auditInfo}", auditInfo);
        return Task.CompletedTask;
    }
}
