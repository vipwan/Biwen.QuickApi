namespace Biwen.QuickApi.Scheduling.Stores.ConfigurationStore
{
    [Obsolete]
    internal class ConfigurationScheduleOptions
    {
        public string ScheduleType { get; set; } = null!;
        public string Cron { get; set; } = null!;
        public string? Description { get; set; }

        public bool IsAsync { get; set; } = false;

        public bool IsStartOnInit { get; set; } = false;

    }
}