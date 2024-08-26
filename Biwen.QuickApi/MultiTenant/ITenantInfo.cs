namespace Biwen.QuickApi.MultiTenant;

/// <summary>
/// 多租户信息
/// </summary>
public interface ITenantInfo
{
    /// <summary>
    /// 租户标识Id忽略大小写
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// 用于鉴别的标识,对于域名等方式的租户鉴别,使用正则表达式
    /// </summary>
    string Identifier { get; set; }

    /// <summary>
    /// 别名
    /// </summary>
    string Name { get; }

}
