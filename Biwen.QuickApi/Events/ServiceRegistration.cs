namespace Biwen.QuickApi.Events
{
    [SuppressType]
    internal static class ServiceRegistration
    {
        /// <summary>
        /// 注册事件相关服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        internal static IServiceCollection AddEvent(this IServiceCollection services)
        {
            //注册EventHanders
            foreach (var subscriberType in EventSubscribers)
            {
                //存在一个订阅者订阅多个事件的情况:
                var baseTypes = subscriberType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == InterfaceEventSubscriber).ToArray();
                foreach (var baseType in baseTypes)
                {
                    services.AddScoped(baseType, subscriberType);
                }
            }
            //注册Publisher
            services.AddActivatedSingleton<Publisher>();
            return services;
        }

        static readonly object _lock = new();//锁
        static IEnumerable<Type> _eventSubscribers = null!;
        static readonly Type InterfaceEventSubscriber = typeof(IEventSubscriber<>);
        static bool IsToGenericInterface(Type type, Type baseInterface)
        {
            if (type == null) return false;
            if (baseInterface == null) return false;

            return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == baseInterface);
        }

        static IEnumerable<Type> EventSubscribers
        {
            get
            {
                lock (_lock)
                    return _eventSubscribers ??= ASS.InAllRequiredAssemblies.Where(x =>
                    !x.IsAbstract && x.IsPublic && x.IsClass && IsToGenericInterface(x, InterfaceEventSubscriber));
            }
        }
    }
}