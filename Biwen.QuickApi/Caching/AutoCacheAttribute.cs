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
    public TimeSpan? Expiration { get; init; }

    /// <summary>
    /// 默认缓存是否启用.默认true
    /// </summary>
    public bool IsEnabled { get; init; } = true;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isEnabled">如果需要禁止缓存可以配置isEnabled:false</param>
    /// <param name="expiration"></param>
    public AutoCacheAttribute(bool isEnabled, int expiration = 1800) : this(expiration)
    {
        IsEnabled = isEnabled;
    }

}
