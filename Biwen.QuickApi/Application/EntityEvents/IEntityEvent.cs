using Biwen.QuickApi.Events;

namespace Biwen.QuickApi.Application.EntityEvents
{
    public interface IEntityEvent : IEvent
    {
    }

    [SuppressType]
    public static class EntityExtensions
    {
        /// <summary>
        /// 通知实体 <typeparamref name="T"/> 添加事件 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static async Task PublishAddedAsync<T>(this T entity) where T : class
        {
            await new EntityAdded<T>(entity).PublishAsync();
        }

        /// <summary>
        /// 通知实体 <typeparamref name="T"/> 更新事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static async Task PublishUpdatedAsync<T>(this T entity) where T : class
        {
            await new EntityUpdated<T>(entity).PublishAsync();
        }

        /// <summary>
        /// 通知实体 <typeparamref name="T"/> 删除事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static async Task PublishDeletedAsync<T>(this T entity) where T : class
        {
            await new EntityDeleted<T>(entity).PublishAsync();
        }
    }
}
