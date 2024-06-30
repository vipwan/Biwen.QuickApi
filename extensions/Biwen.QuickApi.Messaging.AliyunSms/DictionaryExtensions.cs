
namespace Biwen.QuickApi.Messaging.AliyunSms;

internal static class DictionaryExtensions
{
    /// <summary>
    /// 获取字典中的值,如果不存在则返回默认值
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out var obj) ? obj : default;
    }
}
