using Biwen.QuickApi.MultiTenant;
using Biwen.QuickApi.MultiTenant.Abstractions;

namespace Biwen.QuickApi.DemoWeb.MultiTenant
{
    /// <summary>
    /// 自定义租户信息提供者
    /// </summary>
    internal class MyTenantInfoProvider : ITenantInfoProvider<TenantInfo>
    {
        public Task<IList<TenantInfo>> GetAll()
        {
            return Task.FromResult<IList<TenantInfo>>(new List<TenantInfo>
            {
                new TenantInfo
                {
                    Id = "tenant1",
                    Name = "租户1",
                    ConnectionString = "Data Source=.;Initial Catalog=Db1;Integrated Security=True"
                },
                new TenantInfo
                {
                    Id = "tenant2",
                    Name = "租户2",
                    ConnectionString = "Data Source=.;Initial Catalog=Db2;Integrated Security=True"
                }
            });
        }
    }
}
