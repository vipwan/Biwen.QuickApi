namespace Biwen.QuickApi
{
    using Microsoft.AspNetCore.Http;
    using NSwag.Annotations;
    using System.Text.Json;

    [OpenApiIgnore]
    public abstract class BaseResponse
    {
        /// <summary>
        /// ToJson
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
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

        /// <summary>
        /// 直接返回OK
        /// </summary>
        public static IResultResponse OK(object? value = null) => Results.Ok(value).AsRspOfResult();

        /// <summary>
        /// BadRequest
        /// </summary>
        public static IResultResponse BadRequest(object? error = null) => Results.BadRequest(error).AsRspOfResult();

        /// <summary>
        /// Empty
        /// </summary>
        public static IResultResponse Empty => Results.Empty.AsRspOfResult();

        /// <summary>
        /// Stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="contentType"></param>
        /// <param name="fileDownloadName"></param>
        /// <param name="enableRangeProcessing"></param>
        /// <returns></returns>
        public static IResultResponse Stream(
            Stream stream,
            string? contentType = null,
            string? fileDownloadName = null,
            bool enableRangeProcessing = false
            ) =>
            Results.Stream(stream, contentType, fileDownloadName, enableRangeProcessing: enableRangeProcessing).AsRspOfResult();

        /// <summary>
        /// File
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contentType"></param>
        /// <param name="fileDownloadName"></param>
        /// <param name="enableRangeProcessing"></param>
        /// <returns></returns>
        public static IResultResponse File(
            string path,
            string? contentType = null,
            string? fileDownloadName = null,
             bool enableRangeProcessing = false
            ) =>
            Results.File(path, contentType, fileDownloadName, enableRangeProcessing: enableRangeProcessing).AsRspOfResult();

        /// <summary>
        /// NotFound
        /// </summary>
        public static IResultResponse NotFound => Results.NotFound().AsRspOfResult();

        /// <summary>
        /// Unauthorized
        /// </summary>
        public static IResultResponse Unauthorized => Results.Unauthorized().AsRspOfResult();

        /// <summary>
        /// Content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <param name="encoding"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static IResultResponse Content(
            string? content,
            string? contentType = null,
            System.Text.Encoding? encoding = null,
            int? statusCode = null
            ) =>
            Results.Content(content, contentType, encoding, statusCode).AsRspOfResult();


        /// <summary>
        /// Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IResultResponse Json<T>(
            T value,
            JsonSerializerOptions? serializerOptions = null,
            string? contentType = null,
            int? statusCode = null
            ) =>
            Results.Json(value, serializerOptions, contentType, statusCode).AsRspOfResult();


        /// <summary>
        /// StatusCode
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static IResultResponse StatusCode(int statusCode) => Results.StatusCode(statusCode).AsRspOfResult();

    }
}