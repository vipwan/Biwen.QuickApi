using Microsoft.AspNetCore.Http;
namespace Biwen.QuickApi
{
    public static class IResultExtentions
    {
        /// <summary>
        /// 转换为IResultResponse
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IResultResponse AsRsp(this IResult result)
        {
            return new IResultResponse(result ?? throw new ArgumentNullException(nameof(result)));
        }
    }
}