namespace Biwen.QuickApi.Scheduling;

/// <summary>
/// ScheduleTaskAttribute
/// 请注意如果如果ScheduleTaskType&&Cron&&Description&&IsAsync&&IsStartOnInit都相同，会被认为是同一个任务,所以请确保这些属性的唯一性
/// </summary>
/// <param name="cron"></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class ScheduleTaskAttribute(string cron) : Attribute
{

    /// <summary>
    /// Cron表达式,5位码,不支持秒,常用:<see cref="Constants"/>,<seealso href="https://en.wikipedia.org/wiki/Cron"/>
    /// </summary>
    public string Cron { get; set; } = cron;

    /// <summary>
    /// 任务描述信息
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否异步执行.默认:false 会阻塞接下来的同类任务
    /// </summary>
    public bool IsAsync { get; set; } = false;

    /// <summary>
    /// 是否初始化即启动,默认:false
    /// </summary>
    public bool IsStartOnInit { get; set; } = false;

}