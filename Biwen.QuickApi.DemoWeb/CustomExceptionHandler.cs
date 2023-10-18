
using Biwen.QuickApi.Http;

namespace Biwen.QuickApi.DemoWeb
{
    /// <summary>
    /// 自定义异常处理
    /// </summary>
    public class CustomExceptionHandler : IQuickApiExceptionHandler
    {
        private readonly ILogger<CustomExceptionHandler> _logger;
        public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(Exception exception)
        {
            _logger.LogError(exception, "QuickApi异常");
            return Task.CompletedTask;
        }
    }
}