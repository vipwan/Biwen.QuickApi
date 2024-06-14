using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace Biwen.QuickApi.Scheduling.Stores.ConfigurationStore
{
    /// <summary>
    /// 配置文件Store
    /// </summary>
    /// <param name="configuration"></param>
    internal class ConfigurationScheduleMetadataStore(IConfiguration configuration) : IScheduleMetadataStore
    {
        const string Key = "BiwenQuickApi:Schedules";
        private static readonly ConcurrentDictionary<string, ScheduleTaskMetadata> ScheduleTaskTypes = new();

        public Task<ScheduleTaskMetadata[]> GetAllAsync()
        {
            var options = configuration.GetSection(Key).GetChildren();

            if (options?.Any() is true)
            {
                var metadatas = options.Select(x =>
                {
                    //cacheKey = x值的拼接
                    var cacheKey = string.Join(":", x.GetChildren().Select(x => x.Value));

                    return ScheduleTaskTypes.GetOrAdd(cacheKey, _ =>
                    {
                        var type =
                            Type.GetType(x[nameof(ScheduleTaskMetadata.ScheduleTaskType)]!) ??
                            throw new ArgumentException($"Type {x[nameof(ScheduleTaskMetadata.ScheduleTaskType)]} not found!");

                        return new ScheduleTaskMetadata(type, x[nameof(ScheduleTaskMetadata.Cron)]!)
                        {
                            Description = x[nameof(ScheduleTaskMetadata.Description)],
                            IsAsync = string.IsNullOrEmpty(x[nameof(ScheduleTaskMetadata.IsAsync)]) ? false : bool.Parse(x[nameof(ScheduleTaskMetadata.IsAsync)]!),
                            IsStartOnInit = string.IsNullOrEmpty(x[nameof(ScheduleTaskMetadata.IsStartOnInit)]) ? false : bool.Parse(x[nameof(ScheduleTaskMetadata.IsStartOnInit)]!),
                        };
                    });
                }).ToArray();
                return Task.FromResult(metadatas);
            }
            return Task.FromResult(Array.Empty<ScheduleTaskMetadata>());
        }
    }
}