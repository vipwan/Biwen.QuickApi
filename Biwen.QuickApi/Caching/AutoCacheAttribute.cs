namespace Biwen.QuickApi.Caching;

/// <summary>
/// 自动缓存特性
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AutoCacheAttribute : Attribute
{
    /// <summary>
    /// 缓存过期时间
    /// </summary>
    public TimeSpan? Expiration { get; }

    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="expiration">缓存过期时间(s,秒),默认30分钟</param>
    public AutoCacheAttribute(int expiration = 1800)
    {
        if (expiration <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expiration), "缓存过期时间必须大于0s");
        }
        Expiration = TimeSpan.FromSeconds(expiration);
    }
}
