using Biwen.QuickApi.Scheduling.Stores;
using Biwen.QuickApi.Scheduling.Stores.ConfigurationStore;

namespace Biwen.QuickApi.Scheduling
{
    [SuppressType]
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
                services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IScheduleTask), task));

            }

            //调度器
            services.AddScheduler<SampleNCrontabScheduler>();

            //元数据Store
            services.AddScheduleMetadataStore<MetadataStore>();
            //配置文件Store
            services.AddScheduleMetadataStore<ConfigurationScheduleMetadataStore>();
            //BackgroundService
            services.AddHostedService<ScheduleBackgroundService>();
            return services;
        }

        /// <summary>
        /// 注册调度器AddScheduler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddScheduler<T>(this IServiceCollection services) where T : class, IScheduler
        {
            //调度器
            services.AddActivatedSingleton<IScheduler, T>();
            return services;
        }

        /// <summary>
        /// 注册MetadataStore,已内置AttributeStore和ConfigureStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddScheduleMetadataStore<T>(this IServiceCollection services) where T : class, IScheduleMetadataStore
        {
            services.AddActivatedSingleton<IScheduleMetadataStore, T>();
            return services;
        }

        static readonly object _lock = new();//锁
        static IEnumerable<Type> _scheduleTasks = null!;
        static IEnumerable<Type> ScheduleTasks
        {
            get
            {
                lock (_lock)
                    return _scheduleTasks ??= ASS.InAllRequiredAssemblies.ThatInherit<IScheduleTask>()
                        .Where(x => !x.IsAbstract && x.IsClass);
            }
        }
    }
}