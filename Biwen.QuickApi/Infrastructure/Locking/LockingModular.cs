
namespace Biwen.QuickApi.Infrastructure.Locking
{
    [CoreModular]
    internal class LockingModular : ModularBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<LocalLock>();
            services.AddActivatedSingleton<ILocalLock>(sp => sp.GetRequiredService<LocalLock>());
        }
    }
}