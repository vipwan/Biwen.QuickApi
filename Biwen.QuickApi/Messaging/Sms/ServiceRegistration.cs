namespace Biwen.QuickApi.Messaging.Sms
{
    [SuppressType]
    internal static class ServiceRegistration
    {
        /// <summary>
        /// Add SmsSender
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        public static void AddSmsSender<T>(this IServiceCollection services) where T : class, ISmsSender
        {
            services.TryAddActivatedSingleton<ISmsSender, T>();
        }

        /// <summary>
        /// 添加空的短信发送器
        /// </summary>
        /// <param name="services"></param>
        public static void AddNullSmsSender(this IServiceCollection services)
        {
            services.TryAddActivatedSingleton<ISmsSender, NullSmsSender>();
        }
    }
}