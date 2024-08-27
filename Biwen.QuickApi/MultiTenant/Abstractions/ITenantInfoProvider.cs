namespace Biwen.QuickApi.MultiTenant.Abstractions;

/// <summary>
/// 租户信息提供者
/// </summary>
/// <typeparam name="TInfo"></typeparam>
public interface ITenantInfoProvider<TInfo> where TInfo : ITenantInfo
{
    /// <summary>
    /// 请注意如果是通过数据库,配置文件等方式,内部请使用缓存提升性能
    /// </summary>
    /// <returns></returns>
    [AutoCache]
    Task<IList<TInfo>> GetAll();
}