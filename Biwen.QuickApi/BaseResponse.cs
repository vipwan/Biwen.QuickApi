using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi
{
    public abstract class BaseResponse
    {

    }

    /// <summary>
    /// 空输出
    /// </summary>
    public sealed class EmptyResponse : BaseResponse
    {
        public static EmptyResponse New => new();
    }

    /// <summary>
    /// 文本输出
    /// </summary>
    public sealed class ContentResponse : BaseResponse
    {
        public ContentResponse(string content)
        {
            Content = content;
        }

        public string Content { get; set; }


        /// <summary>
        /// Content
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Content;
        }

    }

    /// <summary>
    /// IResult输出.
    /// 针对IResultResponse 请自行重写BaseQuickApi.HandlerBuilder方法的OpenApi实现
    /// </summary>
    public sealed class IResultResponse : BaseResponse
    {
        public IResultResponse(IResult result)
        {
            Result = result;
        }

        public IResult Result { get; set; }
    }
}