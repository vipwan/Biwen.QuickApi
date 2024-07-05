namespace Biwen.QuickApi.Auditing;

internal class ConsoleAuditHandler(ILogger<ConsoleAuditHandler> logger) : IAuditHandler
{
    public Task Handle(AuditInfo auditInfo)
    {
        //仅针对Public方法拦截打印
        if (auditInfo.ActionInfo?.MethodInfo?.IsPublic is true)
        {
            logger.LogTrace("AuditInfo: {@auditInfo}", auditInfo);
        }

        if (auditInfo.IsQuickApi)
        {
            logger.LogInformation("QuickApi: {@Url}", auditInfo.Url);
        }

        return Task.CompletedTask;
    }
}