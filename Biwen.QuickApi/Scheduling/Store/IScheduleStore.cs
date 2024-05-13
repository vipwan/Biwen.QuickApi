namespace Biwen.QuickApi.Scheduling.Store
{

    /// <summary>
    /// 存储Schedule的接口
    /// </summary>
    public interface IScheduleMetadaStore
    {
        /// <summary>
        /// 获取所有ScheduleTaskMetadata
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ScheduleTaskMetadata>> GetAllAsync();
    }
}