using Biwen.QuickApi.MultiTenant.Abstractions;

namespace Biwen.QuickApi.MultiTenant;

public class MultiTenantOptions
{
    public bool Enabled { get; set; } = false;

    public Type? TenantInfoType { get; internal set; }
}
