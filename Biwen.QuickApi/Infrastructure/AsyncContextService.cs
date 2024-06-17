using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AsyncState;

namespace Biwen.QuickApi.Infrastructure
{
    /// <summary>
    /// 异步流Context服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AsyncContextService<T> : IAsyncContext<T> where T : class
    {
        public AsyncContextService(IServiceProvider serviceProvider)
        {
            _asyncState = serviceProvider.GetRequiredService<IAsyncState>()!;
            _asyncContext = serviceProvider.GetRequiredService<IAsyncContext<T>>();

            httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>()!;
            if (httpContextAccessor?.HttpContext is null)
            {
                //如果不是HttpContext请求，初始化AsyncState, 用于非HttpContext调用
                //对于HttpContext请求，异步流存放在HttpContext.Features中
                _asyncState.Initialize();
            }
        }

        private readonly IAsyncState _asyncState;
        private readonly IHttpContextAccessor? httpContextAccessor;
        private readonly IAsyncContext<T> _asyncContext;

        public T? Get()
        {
            return _asyncContext.Get();
        }

        public bool TryGet(out T? value)
        {
            return _asyncContext.TryGet(out value);
        }

        public void Set(T? value)
        {
            _asyncContext.Set(value);
        }
    }
}