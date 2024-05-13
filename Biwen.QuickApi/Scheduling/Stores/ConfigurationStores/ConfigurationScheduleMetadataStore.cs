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
                    var type = Type.GetType(x[nameof(ConfigurationScheduleOption.ScheduleType)]!);
                    if (type is null)
                        throw new ArgumentException($"Type {x[nameof(ConfigurationScheduleOption.ScheduleType)]} not found!");

                    return new ScheduleTaskMetadata(type, x[nameof(ConfigurationScheduleOption.Cron)]!)
                    {
                        Description = x[nameof(ConfigurationScheduleOption.Description)],
                        IsAsync = string.IsNullOrEmpty(x[nameof(ConfigurationScheduleOption.IsAsync)]) ? false : bool.Parse(x[nameof(ConfigurationScheduleOption.IsAsync)]!),
                        IsStartOnInit = string.IsNullOrEmpty(x[nameof(ConfigurationScheduleOption.IsStartOnInit)]) ? false : bool.Parse(x[nameof(ConfigurationScheduleOption.IsStartOnInit)]!),
                    };
                });
                return Task.FromResult(metadatas);
            }
            return Task.FromResult(Enumerable.Empty<ScheduleTaskMetadata>());
        }
    }
}