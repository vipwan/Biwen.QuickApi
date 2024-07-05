namespace Biwen.QuickApi.Messaging.Email;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// Add SmsSender
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    public static void AddEmailSender<T>(this IServiceCollection services) where T : class, IEmailSender
    {
        services.TryAddScoped<IEmailSender, T>();
    }

    /// <summary>
    /// 添加空的邮件发送器
    /// </summary>
    /// <param name="services"></param>
    public static void AddNullEmailSender(this IServiceCollection services)
    {
        services.TryAddScoped<IEmailSender, NullEmailSender>();
    }

    /// <summary>
    /// 返回指定键的短信发送器
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IEmailSender? GetKeyedEmailSender(this IServiceProvider serviceProvider, string key)
    {
        return serviceProvider.GetServices<IEmailSender>().FirstOrDefault(x => x.KeyedName == key);
    }
}