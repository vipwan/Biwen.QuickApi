using Biwen.QuickApi.Scheduling.Stores;
using Biwen.QuickApi.Scheduling.Stores.ConfigurationStore;

namespace Biwen.QuickApi.Scheduling
{
    public static class ServiceRegistration
    {
        /// <summary>
        /// AddScheduleTask
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        internal static IServiceCollection AddScheduleTask(this IServiceCollection services)
        {
            foreach (var task in ScheduleTasks)
            {
                services.AddTransient(task);
                services.AddTransient(typeof(IScheduleTask), task);
            }

            //配置文件Store
            services.AddScheduleMetadaStore<ConfigurationScheduleMetadataStore>();

            services.AddHostedService<ScheduleBackgroundService>();
            return services;
        }

        /// <summary>
        /// 注册ScheduleMetadataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddScheduleMetadaStore<T>(this IServiceCollection services) where T : class, IScheduleMetadataStore
        {
            services.AddSingleton(typeof(IScheduleMetadataStore), typeof(T));
            return services;
        }

        static readonly object _lock = new();//锁
        static readonly Type InterfaceScheduleTask = typeof(IScheduleTask);

        static IEnumerable<Type> _scheduleTasks = null!;
        static IEnumerable<Type> ScheduleTasks
        {
            get
            {
                lock (_lock)
                    return _scheduleTasks ??= ASS.InAllRequiredAssemblies.Where(x =>
                    !x.IsAbstract && x.IsPublic && x.IsClass && x.GetInterfaces().Any(x => x == InterfaceScheduleTask));
            }
        }
    }
}