using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Biwen.QuickApi.DemoWeb.Apis
{
    public class BaseEntity
    {
        public int Id { get; set; }
    }

    public class UserInfo : BaseEntity
    {
        public string? Name { get; set; }
        public int Age { get; set; }

        public string? Remark { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public string? EscapedCol { get; set; }
    }


    [AutoDto(typeof(UserInfo), nameof(UserInfo.Remark), "EscapedCol")]
    public partial class UserInfoDto { }

    [AutoDto<UserInfo>(nameof(UserInfo.Remark), "EscapedCol")]
    public partial class UserInfo2Dto { }


    /// <summary>
    /// 分页
    /// </summary>
    public interface IPager
    {
        /// <summary>
        /// 页码
        /// </summary>
        [DefaultValue(0), Description("页码,从0开始")]
        [Range(0, int.MaxValue)]
        int? CurrentPage { get; set; }

        /// <summary>
        /// 分页项数
        /// </summary>
        [DefaultValue(10), Description("每页项数,10-30之间")]
        [Range(10, 30)]
        int? PageLen { get; set; }

    }

    /// <summary>
    /// 查询
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// 关键字
        /// </summary>
        [StringLength(100), Description("查询关键字")]
        string? KeyWord { get; set; }
    }

    /// <summary>
    /// 使用Biwen.AutoClassGen生成的Request
    /// </summary>
    [AutoGen("AutoGenRequest", "Biwen.QuickApi.DemoWeb.Apis")]
    public interface IAutoGenRequest : IPager, IQuery { }

    [FromBody]
    public partial class AutoGenRequest : BaseRequest<AutoGenRequest>
    {
    }

    [QuickApi("autogen", Verbs = Verb.POST)]
    [OpenApiMetadata("自动生成的Request", "自动生成的接口")]
    [EndpointGroupName("group1")]
    public class AutoGenApi : BaseQuickApi<AutoGenRequest, IResult>
    {
        public override async ValueTask<IResult> ExecuteAsync(AutoGenRequest request)
        {
            await Task.CompletedTask;

            UserInfoDto userinfo = new()
            {
                Name = "张三",
                Age = 18,
                Address = "北京市朝阳区",
                Phone = "13888888888"
            };

            return Results.Json(request);
        }
    }
}