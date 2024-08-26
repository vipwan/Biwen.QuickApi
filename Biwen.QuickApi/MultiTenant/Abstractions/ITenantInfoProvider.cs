namespace Biwen.QuickApi.MultiTenant.Abstractions;

public interface ITenantInfoProvider<T> where T : ITenantInfo
{
    /// <summary>
    /// 请注意如果是通过数据库,配置文件等方式,内部请使用缓存提升性能
    /// </summary>
    /// <returns></returns>
    Task<IList<T>> GetAll();
}