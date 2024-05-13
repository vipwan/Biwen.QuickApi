namespace Biwen.QuickApi.Scheduling.Stores
{

    /// <summary>
    /// 存储Schedule的接口,可以自行扩展配置文件或者数据库存储
    /// </summary>
    public interface IScheduleMetadataStore
    {
        /// <summary>
        /// 获取所有ScheduleTaskMetadata
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ScheduleTaskMetadata>> GetAllAsync();
    }
}