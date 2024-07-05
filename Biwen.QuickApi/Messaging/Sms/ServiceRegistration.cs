namespace Biwen.QuickApi.Messaging.Sms;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// Add SmsSender
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    public static void AddSmsSender<T>(this IServiceCollection services) where T : class, ISmsSender
    {
        services.TryAddScoped<ISmsSender, T>();
    }

    /// <summary>
    /// 添加空的短信发送器
    /// </summary>
    /// <param name="services"></param>
    public static void AddNullSmsSender(this IServiceCollection services)
    {
        services.TryAddScoped<ISmsSender, NullSmsSender>();
    }

    /// <summary>
    /// 返回指定键的短信发送器
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static ISmsSender? GetKeyedSmsSender(this IServiceProvider serviceProvider, string key)
    {
        return serviceProvider.GetServices<ISmsSender>().FirstOrDefault(x => x.KeyedName == key);
    }
}