using Biwen.QuickApi.Events;
using Biwen.QuickApi.Infrastructure.Locking;

namespace Biwen.QuickApi.Scheduling;

[CoreModular, PreModular<LockingModular, EventsModular>]
internal class SchedulingModular(IOptions<BiwenQuickApiOptions> options) : ModularBase
{

    public override Func<bool> IsEnable => () => options.Value.EnableScheduling;

    public override void ConfigureServices(IServiceCollection services)
    {
        var enablePubSub = options.Value.EnablePubSub;
        if (!enablePubSub) throw new QuickApiExcetion("必须启用发布订阅功能,才可以开启Scheduling功能!");

        services.AddScheduleTask();
    }
}