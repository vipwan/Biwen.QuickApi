using Biwen.QuickApi.Infrastructure;
using Biwen.QuickApi.Messaging.Sms;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.Messaging.AliyunSms;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// Add AliyunSmsSender
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    public static void AddAliyunSmsSender(this IServiceCollection services, Action<AliyunSmsOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        services.Configure(options);
        services.AddSmsSender<AliyunSmsSender>();
    }
}