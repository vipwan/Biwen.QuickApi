namespace Biwen.QuickApi.Scheduling
{
    /// <summary>
    /// Constants for Scheduling Cron
    /// </summary>
    [SuppressType]
    public static class Constants
    {
        /// <summary>
        /// Cron表达式,每分钟
        /// </summary>
        public const string CronEveryMinute = "* * * * *";

        /// <summary>
        /// Cron表达式,每15分钟
        /// </summary>
        public const string CronEvery15Minutes = "0/15 * * * *";

        /// <summary>
        /// Cron表达式,每N分钟
        /// </summary>
        /// <param name="n">1~59</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static string CronEveryNMinutes(this int n) => n switch
        {
            (>= 1 and < 60) => $"0/{n} * * * *",
            _ => throw new ArgumentException("n must be between 1 and 60", nameof(n))
        };

        /// <summary>
        /// Cron表达式,每N小时
        /// </summary>
        /// <param name="n">1~23</param>
        /// <returns></returns>
        public static string CronEveryNHours(this int n) => n switch
        {
            (>= 1 and < 24) => $"0 0/{n} * * *",
            _ => throw new ArgumentException("n must be between 1 and 24", nameof(n))
        };

        /// <summary>
        /// Cron表达式,每小时
        /// </summary>
        public const string CronEveryHour = "0 * * * *";

        /// <summary>
        /// Cron表达式,每天
        /// </summary>
        public const string CronEveryDay = "0 0 * * *";

        /// <summary>
        /// Cron表达式,每周
        /// </summary>
        public const string CronEveryWeek = "0 0 * * 0";

        /// <summary>
        /// Cron表达式,每月(不推荐使用)
        /// </summary>
        public const string CronEveryMonth = "0 0 1 * *";

        /// <summary>
        /// Cron表达式,每年(不推荐使用)
        /// </summary>
        public const string CronEveryYear = "0 0 1 1 *";

        /// <summary>
        /// 对秒级别的支持.自定义格式,且只支持10~59秒
        /// </summary>
        public const string SecondFormat = "{0}:SECONDS";

        public static string CronForSeconds(this int seconds) => seconds switch
        {
            (>= 10 and < 60) => string.Format(SecondFormat, seconds),
            _ => throw new ArgumentException("seconds must be between 10 and 59", nameof(seconds))
        };

    }
}