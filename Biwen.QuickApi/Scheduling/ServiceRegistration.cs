namespace Biwen.QuickApi.Scheduling
{
    internal static class ServiceRegistration
    {
        /// <summary>
        /// AddScheduleTask
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddScheduleTask(this IServiceCollection services)
        {
            foreach (var task in ScheduleTasks)
            {
                services.AddSingleton(task);
                services.AddSingleton(typeof(IScheduleTask), task);
            }
            services.AddHostedService<ScheduleBackgroundService>();
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