using Biwen.QuickApi.MultiTenant.Abstractions;

namespace Biwen.QuickApi.MultiTenant.InfoProviders;

/// <summary>
/// 通过配置文件提供租户信息
/// </summary>
/// <typeparam name="TInfo"></typeparam>
/// <param name="configuration"></param>
public class ConfigurationInfoProvider<TInfo>(IConfiguration configuration) :
    ITenantInfoProvider<TInfo>
    where TInfo : class, ITenantInfo
{
    internal static volatile string DefaultSectionName = "BiwenQuickApi:MultiTenants";

    public virtual Task<IList<TInfo>> GetAllAsync()
    {
        var section = configuration.GetSection(DefaultSectionName);

        var children = section.GetChildren();
        var list = new List<TInfo>();

        if (children?.Any() is true)
        {
            foreach (var child in children)
            {
                var info = child.Get<TInfo>();
                if (info != null)
                {
                    list.Add(info);
                }
            }
        }
        return Task.FromResult<IList<TInfo>>(list);
    }
}