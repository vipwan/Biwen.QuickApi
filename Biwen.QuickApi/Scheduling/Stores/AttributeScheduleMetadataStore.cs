using Biwen.QuickApi.Caching;

namespace Biwen.QuickApi.Scheduling.Stores
{
    /// <summary>
    /// MetadataStore
    /// </summary>
    internal sealed class AttributeScheduleMetadataStore : IScheduleMetadataStore
    {
        /// <summary>
        /// 特性中的metadatas缓存起来
        /// </summary>
        private static ScheduleTaskMetadata[] _cachedMetatas = [];

        public AttributeScheduleMetadataStore(IServiceProvider serviceProvider)
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
                        _cachedMetatas = [.._cachedMetatas, .. metadatas.Select(metadata =>
                            new ScheduleTaskMetadata(taskType, metadata.Cron)
                            {
                                Description = metadata.Description,
                                IsAsync = metadata.IsAsync,
                                IsStartOnInit = metadata.IsStartOnInit,
                        })];
                    }
                }
            }
        }

        [AutoCache(int.MaxValue)]
        public Task<ScheduleTaskMetadata[]> GetAllAsync()
        {
            return Task.FromResult(_cachedMetatas);
        }
    }
}