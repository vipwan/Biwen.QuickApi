using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Abstractions
{
    /// <summary>
    /// 请注意IReqBinder不支持构造器注入
    /// 如果需要DI,使用HttpContext.RequestServices获取Service
    /// 请注意IReqBinder,会导致Swagger无法生成正确的Schema,请务必重写<see cref="BaseQuickApi.HandlerBuilder(RouteHandlerBuilder)"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReqBinder<T> where T : class, new()
    {
        static abstract Task<T> BindAsync(HttpContext context, ParameterInfo parameter = null!);
    }
}