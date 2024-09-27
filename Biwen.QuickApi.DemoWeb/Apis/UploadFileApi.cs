using System.ComponentModel.DataAnnotations;

namespace Biwen.QuickApi.DemoWeb.Apis
{

    /// <summary>
    /// 上传文件请求
    /// </summary>
    public class UploadFileRequest : BaseRequest<UploadFileRequest>
    {
        /// <summary>
        /// 文件
        /// </summary>
        [Required]
        public IFormFile File { get; set; } = null!;

        /// <summary>
        /// 保存类型.0:本地,1:阿里云
        /// </summary>
        [FromHeader]
        public int SaveType { get; set; } = 0;
    }

    /// <summary>
    /// 上传文件响应
    /// </summary>
    /// <param name="Flag"></param>
    /// <param name="UrlPath"></param>
    public record UploadFileResponse(bool Flag, string? Error, string? UrlPath, TimeSpan PostTime);



    [QuickApi("upload-file", Verbs = Verb.POST)]
    public class UploadFileApi : BaseQuickApi<UploadFileRequest, UploadFileResponse>
    {
        public override async ValueTask<UploadFileResponse> ExecuteAsync(UploadFileRequest request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return new UploadFileResponse(true, null, "https://github.com/vipwan", TimeSpan.FromSeconds(5));
        }
    }
}