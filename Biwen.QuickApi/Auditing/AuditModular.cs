namespace Biwen.QuickApi.Auditing;

[CoreModular]
internal class AuditModular : ModularBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAuditHandler<ConsoleAuditHandler>();
        //注入审计代理
        services.TryAddSingleton(typeof(AuditProxyFactory<>));
    }

}
