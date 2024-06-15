using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi
{
    [SuppressType]
    public static class IResultExtensions
    {
        /// <summary>
        /// 转换为IResultResponse
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
#pragma warning disable CS0618 // 类型或成员已过时
        public static IResultResponse AsRspOfResult(this IResult result)
        {
            return new IResultResponse(result ?? throw new ArgumentNullException(nameof(result)));
        }
    }
#pragma warning restore CS0618 // 类型或成员已过时

}