﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace Biwen.QuickApi
{
    public static class IEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// 扩展MapMethods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="app"></param>
        /// <param name="pattern"></param>
        /// <param name="excludeFromDescription">是否从文档中排除</param>
        /// <returns></returns>
        public static RouteHandlerBuilder MapMethods<T>(this IEndpointRouteBuilder app, [StringSyntax("Route")] string pattern, bool excludeFromDescription = false) where T : class, IQuickEndpoint
        {
            var verbs = T.Verbs.SplitEnum();
            var builder = app.MapMethods(pattern, verbs.Select(x => x.ToString()), T.Handler);
            //获取T的所有Attribute:
            var attrs = typeof(T).GetCustomAttributes(true);
            //将所有的Attribute添加到metadatas中
            builder.WithMetadata(attrs);
            //添加验证器Filter:
            builder.AddEndpointFilter<ValidateRequestFilter>();
            //添加OpenApi:
            if (excludeFromDescription)
            {
                builder.ExcludeFromDescription();
            }
            return builder;
        }

        //验证Request的Endpoint Filter
        private class ValidateRequestFilter : IEndpointFilter
        {
            public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
            {
                var httpContext = context.HttpContext;
                if (httpContext != null)
                {
                    //验证Request:
                    var args = context.Arguments;
                    foreach (var arg in args)
                    {
                        if (arg is IReqValidator { } validator)
                        {
                            //验证Req
                            if (validator.Validate() is ValidationResult { IsValid: false } vresult)
                            {
                                return TypedResults.ValidationProblem(vresult.ToDictionary());
                            }
                        }
                    }
                }
                return await next(context);
            }
        }

        /// <summary>
        /// MapGroup
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="prefix"></param>
        /// <param name="action"></param>
        public static void MapGroup(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string prefix, Action<IEndpointRouteBuilder> action)
        {
            var group = endpoints.MapGroup(prefix);
            action(group);
        }
    }
}
