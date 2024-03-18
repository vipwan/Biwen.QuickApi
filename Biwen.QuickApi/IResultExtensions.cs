using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi
{
    public static class IResultExtensions
    {
        /// <summary>
        /// 转换为IResultResponse
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IResultResponse AsRspOfResult(this IResult result)
        {
            return new IResultResponse(result ?? throw new ArgumentNullException(nameof(result)));
        }

        /// <summary>
        /// 转换为IResultResponse
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IResultResponse AsRspOfResult(this string result)
        {
            return new IResultResponse(TypedResults.Content(result));
        }

        ///// <summary>
        ///// 转换为ContentResponse
        ///// </summary>
        ///// <param name="result"></param>
        ///// <returns></returns>
        //public static ContentResponse AsRspOfContent(this string result)
        //{
        //    return new ContentResponse(result);
        //}
    }
}