
namespace Biwen.QuickApi.Service.EntityEvents;

/// <summary>
/// 实体添加事件
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public class EntityAdded<TEvent> : IEntityEvent where TEvent : class
{
    public TEvent Entity { get; }

    public EntityAdded(TEvent entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Entity = entity;
    }
}
