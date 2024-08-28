using Biwen.QuickApi.MultiTenant.Abstractions;

namespace Biwen.QuickApi.MultiTenant.InfoProviders;

/// <summary>
/// 基于内存提供租户信息
/// </summary>
/// <typeparam name="TInfo"></typeparam>
internal class MemoryInfoProvider<TInfo>() : ITenantInfoProvider<TInfo>
    where TInfo : class, ITenantInfo
{
    internal static volatile IList<TInfo>? _infos;

    public Task<IList<TInfo>> GetAllAsync()
    {
        return Task.FromResult(_infos ?? []);
    }
}