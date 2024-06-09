namespace Biwen.QuickApi.Events
{

    [CoreModular]
    internal class EventsModular(IServiceProvider serviceProvider) : ModularBase
    {
        public override Func<bool> IsEnable => () =>
        serviceProvider.GetRequiredService<IOptions<BiwenQuickApiOptions>>().Value.EnablePubSub;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddEvent();
        }
    }
}