namespace Biwen.QuickApi.Scheduling.Stores.ConfigurationStore
{
    public class ConfigurationScheduleOption
    {
        public string ScheduleType { get; set; } = null!;
        public string Cron { get; set; } = null!;
        public string? Description { get; set; }

        public bool IsAsync { get; set; } = false;

        public bool IsStartOnInit { get; set; } = false;

    }
}