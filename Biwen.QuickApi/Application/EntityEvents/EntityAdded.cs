namespace Biwen.QuickApi.Application.EntityEvents;

/// <summary>
/// 实体添加事件
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EntityAdded<TEntity> : IEntityEvent where TEntity : class
{
    public TEntity Entity { get; }

    public EntityAdded(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Entity = entity;
    }
}
