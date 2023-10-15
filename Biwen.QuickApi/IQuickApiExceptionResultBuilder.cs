
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi
{

    /// <summary>
    /// 500异常结果
    /// </summary>
    public interface IQuickApiExceptionResultBuilder
    {
        /// <summary>
        /// 规范化返回异常
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        Task<IResult> ErrorResult(Exception exception);
    }


    internal class DefaultExceptionResultBuilder : IQuickApiExceptionResultBuilder
    {
        public Task<IResult> ErrorResult(Exception exception)
        {
            return Task.FromResult(TypedResults.Problem(exception.Message) as IResult);
        }
    }

}