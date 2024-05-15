namespace Biwen.QuickApi.Scheduling.Stores
{
    /// <summary>
    /// MetadataStore
    /// </summary>
    /// <param name="serviceProvider"></param>
    internal sealed class MetadataStore(IServiceProvider serviceProvider) : IScheduleMetadataStore
    {
        /// <summary>
        /// 特性中的metadatas缓存起来
        /// </summary>
        private static List<ScheduleTaskMetadata> _cachedMetatas = null!;

        private static object _lock = new object();//锁

        public Task<IEnumerable<ScheduleTaskMetadata>> GetAllAsync()
        {
            if (_cachedMetatas is not null)
                return Task.FromResult(_cachedMetatas.AsEnumerable());

            lock (_lock)
            {
                _cachedMetatas = [];

                var tasks = serviceProvider.GetServices<IScheduleTask>();
                if (tasks is null || !tasks.Any())
                {
                    return Task.FromResult(Enumerable.Empty<ScheduleTaskMetadata>());
                }

                //注解中的task
                foreach (var task in tasks)
                {
                    var taskType = task.GetType();
                    //标注的metadatas
                    var metadatas = taskType.GetCustomAttributes<ScheduleTaskAttribute>();
                    if (metadatas.Any())
                    {
                        _cachedMetatas.AddRange(metadatas.Select(metadata =>
                        new ScheduleTaskMetadata(taskType, metadata.Cron)
                        {
                            Description = metadata.Description,
                            IsAsync = metadata.IsAsync,
                            IsStartOnInit = metadata.IsStartOnInit,
                        }));
                    }
                }
            }

            return Task.FromResult(_cachedMetatas.AsEnumerable());
        }
    }
}