namespace Biwen.QuickApi.MultiTenant.Abstractions;

/// <summary>
/// 租户查找器
/// </summary>
public interface ITenantFinder<TInfo> where TInfo : ITenantInfo
{
    /// <summary>
    /// 查找租户信息,如果找不到返回null
    /// </summary>
    /// <returns></returns>
    Task<TInfo?> FindAsync();
}