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
}