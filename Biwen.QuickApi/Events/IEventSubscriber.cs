namespace Biwen.QuickApi.Events
{
    public interface IEventSubscriber<T> where T : IEvent
    {
        Task HandleAsync(T @event, CancellationToken ct);

        ///// <summary>
        ///// 执行排序
        ///// </summary>
        //int Order { get; }

        ///// <summary>
        ///// 如果发生错误是否抛出异常,将阻塞后续Handler
        ///// </summary>
        //bool ThrowIfError { get; }
    }


    public abstract class EventSubscriber<T> : IEventSubscriber<T> where T : IEvent
    {
        public abstract Task HandleAsync(T @event, CancellationToken ct);

        //public virtual int Order => 0;
        ///// <summary>
        ///// 默认不抛出异常
        ///// </summary>
        //public virtual bool ThrowIfError => false;
    }

}