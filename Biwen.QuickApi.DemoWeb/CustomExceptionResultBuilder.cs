
using Biwen.QuickApi.Http;

namespace Biwen.QuickApi.DemoWeb
{

    /// <summary>
    /// 自定义异常返回结果
    /// </summary>
    public class CustomExceptionResultBuilder : IQuickApiExceptionResultBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CustomExceptionResultBuilder(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<IResult> ErrorResult(Exception exception)
        {
            return Task.FromResult<IResult>(TypedResults.Json(new
            {
                Status = 500,
                CurrentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                Exception = new
                {
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                }
            }));
        }
    }
}