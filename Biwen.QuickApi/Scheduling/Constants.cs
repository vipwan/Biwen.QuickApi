namespace Biwen.QuickApi.Scheduling
{
    /// <summary>
    /// Constants for Scheduling Cron
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Cron for every minute
        /// </summary>
        public const string CronEveryMinute = "* * * * *";

        /// <summary>
        /// Cron for every 15 minutes
        /// </summary>
        public const string CronEvery15Minutes = "0/15 * * * *";

        /// <summary>
        /// Cron for every N minutes
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
        /// Cron for every N hours
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string CronEveryNHours(this int n) => n switch
        {
            (>= 1 and < 24) => $"0 0/{n} * * *",
            _ => throw new ArgumentException("n must be between 1 and 24", nameof(n))
        };

        /// <summary>
        /// Cron for every hour
        /// </summary>
        public const string CronEveryHour = "0 * * * *";

        /// <summary>
        /// Cron for every day
        /// </summary>
        public const string CronEveryDay = "0 0 * * *";

        /// <summary>
        /// Cron for every week
        /// </summary>
        public const string CronEveryWeek = "0 0 * * 0";

        /// <summary>
        /// Cron for every month
        /// </summary>
        public const string CronEveryMonth = "0 0 1 * *";

        /// <summary>
        /// Cron for every year
        /// </summary>
        public const string CronEveryYear = "0 0 1 1 *";
    }
}