using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
namespace Biwen.QuickApi.Http
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
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly IHostEnvironment _environment;
        private readonly IHttpContextAccessor _contextAccessor;
        public DefaultExceptionResultBuilder(
            IProblemDetailsService problemDetailsService,
            IHostEnvironment environment,
            IHttpContextAccessor contextAccessor)
        {
            _problemDetailsService = problemDetailsService;
            _environment = environment;
            _contextAccessor = contextAccessor;
        }

        public async Task<IResult> ErrorResult(Exception exception)
        {
            await Task.CompletedTask;

            if (_environment.IsProduction())
            {
                return TypedResults.Problem(exception.Message);
            }

            Dictionary<string, object?> result = new();

            result.Add("Status", 500);
            result.Add("CurrentUser", _contextAccessor.HttpContext?.User?.Identity?.Name);
            result.Add("Exception", new
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
            });
            result.Add("RequestPath", _contextAccessor.HttpContext?.Request?.Path.Value);
            result.Add("Method", _contextAccessor.HttpContext?.Request.Method);
            result.Add("QueryString", _contextAccessor.HttpContext?.Request.QueryString.Value);
            //result.Add("Headers", _contextAccessor.HttpContext?.Request.Headers);
            //result.Add("Cookies", _contextAccessor.HttpContext?.Request.Cookies);
            //result.Add("Form", _contextAccessor.HttpContext?.Request.Form);
            //result.Add("RouteValues", _contextAccessor.HttpContext?.Request.RouteValues);
            //result.Add("Query", _contextAccessor.HttpContext?.Request.Query);

            return TypedResults.Problem(exception.Message, extensions: result);

            ////如果不是生产环境,则返回详细的异常信息
            //await _problemDetailsService.WriteAsync(new ProblemDetailsContext
            //{
            //    HttpContext = _contextAccessor?.HttpContext!,
            //    ProblemDetails =
            //    {
            //        Title = "错误详情",
            //        Detail = exception.ToString(),
            //        Type = exception.Message ,
            //        Status =500,
            //        Instance = _contextAccessor?.HttpContext?.Request?.Path
            //    }
            //});

        }
    }
}