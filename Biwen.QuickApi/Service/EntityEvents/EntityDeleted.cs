namespace Biwen.QuickApi.Service.EntityEvents;

/// <summary>
/// 实体删除事件
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EntityDeleted<TEntity> : IEntityEvent where TEntity : class
{
    public TEntity Entity { get; }

    public EntityDeleted(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Entity = entity;
    }
}