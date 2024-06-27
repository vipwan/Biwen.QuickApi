namespace Biwen.QuickApi.Service.EntityEvents;

/// <summary>
/// 实体更新事件
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EntityUpdated<TEntity> : IEntityEvent where TEntity : class
{
    public TEntity Entity { get; }

    public EntityUpdated(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Entity = entity;
    }
}
