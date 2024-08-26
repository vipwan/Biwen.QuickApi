namespace Biwen.QuickApi.MultiTenant;

public class MultiTenantOptions
{
    public bool Enabled { get; internal set; } = true;

    public Type? TenantInfoType { get; internal set; }

    /// <summary>
    /// 默认的租户标识符,如果没有找到租户信息,则使用Id,
    /// 默认为null表示不使用,表示如果没有找到租户信息,则不进行处理
    /// </summary>
    public string? DefaultId { get; set; } = null;

}
