using Biwen.QuickApi.Serializer;

namespace Biwen.QuickApi.Auditing;

[Serializable]
public class AuditInfo
{
    public string? ApplicationName { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? ClientIpAddress { get; set; }

    public string? ClientName { get; set; }

    public string? BrowserInfo { get; set; }

    public string? HttpMethod { get; set; }

    public string? Url { get; set; }

    public ActionInfo? ActionInfo { get; set; }

    /// <summary>
    /// 扩展信息
    /// </summary>
    public Dictionary<string, object?>? ExtraInfos { get; set; }

    /// <summary>
    /// JSON序列化
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return DefaultSerializer.Instance.SerializeToString(this)!;
    }

}

[Serializable]
public class ActionInfo
{
    public string? MethodName { get; set; }

    public string? ClassName { get; set; }

    public string? Namespace { get; set; }
}
