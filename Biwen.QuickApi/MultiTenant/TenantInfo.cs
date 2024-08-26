namespace Biwen.QuickApi.MultiTenant;

/// <summary>
/// 默认的租户信息
/// </summary>
public record class TenantInfo : ITenantInfo
{
    private string id = null!;

    public string Id
    {
        get
        {
            return id;
        }
        set
        {
            if (value != null)
            {
                if (value.Length > 256)
                {
                    throw new QuickApiExcetion($"The tenant id cannot exceed {256} characters.");
                }
                id = value;
            }
        }
    }

    public required string Identifier { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// 链接字符串
    /// </summary>
    public string? ConnectionString { get; set; }
}