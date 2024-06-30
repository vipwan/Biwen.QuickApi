namespace Biwen.QuickApi.Messaging;

/// <summary>
/// INamed <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface INamed<T>
{
    /// <summary>
    /// 针对短信发送器/邮箱发送器等的键名.可能存在多个短信发送器,用于区分
    /// </summary>
    T KeyedName { get; }
}
