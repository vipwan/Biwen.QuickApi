namespace Biwen.QuickApi.Scheduling.Stores
{
    /// <summary>
    /// 存储调度任务Metadata的接口,可以自行扩展配置文件或者数据库存储
    /// </summary>
    public interface IScheduleMetadataStore
    {
        /// <summary>
        /// 获取所有配置任务的Metadata
        /// </summary>
        /// <returns></returns>
        Task<ScheduleTaskMetadata[]> GetAllAsync();
    }
}