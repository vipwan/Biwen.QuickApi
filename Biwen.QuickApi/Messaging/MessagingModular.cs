using Biwen.QuickApi.Messaging.Email;
using Biwen.QuickApi.Messaging.Sms;

namespace Biwen.QuickApi.Messaging;

[CoreModular]
internal class MessagingModular : ModularBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNullSmsSender();
        services.AddNullEmailSender();
    }
}