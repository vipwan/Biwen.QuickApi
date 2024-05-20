using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Abstractions
{
    /// <summary>
    /// IReqBinder绑定器约定接口,当前同时支持QuickApi和MinimalApi
    /// 请注意IReqBinder,会导致Swagger无法生成正确的Schema,请务必重写<see cref="BaseQuickApi.HandlerBuilder(RouteHandlerBuilder)"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReqBinder<T> where T : class, new()
    {
        static abstract ValueTask<T?> BindAsync(HttpContext context, ParameterInfo parameter = null!);
    }
}