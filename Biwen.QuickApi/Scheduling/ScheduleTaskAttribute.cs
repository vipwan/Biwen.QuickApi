namespace Biwen.QuickApi.Scheduling
{
    /// <summary>
    /// ScheduleTaskAttribute
    /// 请注意如果如果ScheduleTaskType&&Cron&&Description&&IsAsync&&IsStartOnInit都相同，会被认为是同一个任务,所以请确保这些属性的唯一性
    /// </summary>
    /// <param name="cron"></param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ScheduleTaskAttribute(string cron) : Attribute
    {
        /// <summary>
        /// 支持的cron表达式格式 * * * * *：https://en.wikipedia.org/wiki/Cron
        /// 最小单位为分钟
        /// </summary>
        public string Cron { get; set; } = cron;

        public string? Description { get; set; }

        /// <summary>
        /// 是否异步执行.默认false会阻塞接下来的同类任务
        /// </summary>
        public bool IsAsync { get; set; } = false;

        /// <summary>
        /// 是否初始化即启动,默认false
        /// </summary>
        public bool IsStartOnInit { get; set; } = false;


    }
}