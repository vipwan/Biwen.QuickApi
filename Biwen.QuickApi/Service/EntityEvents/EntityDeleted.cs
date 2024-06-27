namespace Biwen.QuickApi.Service.EntityEvents;

/// <summary>
/// 实体删除事件
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public class EntityDeleted<TEvent> : IEntityEvent where TEvent : class
{
    public TEvent Entity { get; }

    public EntityDeleted(TEvent entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Entity = entity;
    }
}