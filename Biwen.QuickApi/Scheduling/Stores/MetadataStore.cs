namespace Biwen.QuickApi.Scheduling.Stores
{
    /// <summary>
    /// MetadataStore
    /// </summary>
    internal sealed class MetadataStore : IScheduleMetadataStore
    {
        /// <summary>
        /// 特性中的metadatas缓存起来
        /// </summary>
        private static List<ScheduleTaskMetadata> _cachedMetatas = [];

        public MetadataStore(IServiceProvider serviceProvider)
        {
            lock (this)
            {
                var tasks = serviceProvider.GetServices<IScheduleTask>();
                if (tasks is null || !tasks.Any())
                {
                    return;
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
        }

        public Task<ScheduleTaskMetadata[]> GetAllAsync()
        {
            return Task.FromResult(_cachedMetatas.ToArray());
        }
    }
}