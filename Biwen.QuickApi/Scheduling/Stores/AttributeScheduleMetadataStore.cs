// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:52:50 AttributeScheduleMetadataStore.cs

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