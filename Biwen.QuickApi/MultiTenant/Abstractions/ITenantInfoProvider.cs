namespace Biwen.QuickApi.MultiTenant.Abstractions
{
    public interface ITenantInfoProvider<T> where T : ITenantInfo
    {
        Task<IList<T>> GetAll();
    }
}