using Microsoft.Extensions.Configuration;

namespace Biwen.QuickApi.Scheduling.Stores.ConfigurationStore
{
    /// <summary>
    /// 配置文件Store
    /// </summary>
    /// <param name="configuration"></param>
    internal class ConfigurationScheduleMetadataStore(IConfiguration configuration) : IScheduleMetadataStore
    {
        const string Key = "BiwenQuickApi:Schedules";

        public Task<IEnumerable<ScheduleTaskMetadata>> GetAllAsync()
        {
            var options = configuration.GetSection(Key).GetChildren();

            if (options?.Any() is true)
            {
                var metadatas = options.Select(x =>
                {
                    var type = Type.GetType(x[nameof(ScheduleTaskMetadata.ScheduleTaskType)]!);
                    if (type is null)
                        throw new ArgumentException($"Type {x[nameof(ScheduleTaskMetadata.ScheduleTaskType)]} not found!");

                    return new ScheduleTaskMetadata(type, x[nameof(ScheduleTaskMetadata.Cron)]!)
                    {
                        Description = x[nameof(ScheduleTaskMetadata.Description)],
                        IsAsync = string.IsNullOrEmpty(x[nameof(ScheduleTaskMetadata.IsAsync)]) ? false : bool.Parse(x[nameof(ScheduleTaskMetadata.IsAsync)]!),
                        IsStartOnInit = string.IsNullOrEmpty(x[nameof(ScheduleTaskMetadata.IsStartOnInit)]) ? false : bool.Parse(x[nameof(ScheduleTaskMetadata.IsStartOnInit)]!),
                    };
                });
                return Task.FromResult(metadatas);
            }
            return Task.FromResult(Enumerable.Empty<ScheduleTaskMetadata>());
        }
    }
}