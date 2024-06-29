namespace Biwen.QuickApi.Events
{
    [CoreModular]
    internal class EventsModular(IOptions<BiwenQuickApiOptions> options) : ModularBase
    {
        public override Func<bool> IsEnable => () => options.Value.EnablePubSub;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddEvent();
        }
    }
}