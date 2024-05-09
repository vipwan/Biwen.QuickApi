using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Text;
using System.Text.Json;

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
        Task<T> BindAsync(HttpContext context);
    }
}