using Microsoft.AspNetCore.Builder;
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
        /// <returns></returns>
        public static RouteHandlerBuilder MapMethods<T>(this IEndpointRouteBuilder app, [StringSyntax("Route")] string pattern) where T : class, IQuickEndpoint
        {
            var verbs = T.Verbs.SplitEnum();
            var builder = app.MapMethods(pattern, verbs.Select(x => x.ToString()), T.Handler);
            //获取T的所有Attribute:
            var attrs = typeof(T).GetCustomAttributes(true);
            //将所有的Attribute添加到metadatas中
            builder.WithMetadata(attrs);
            return builder;
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
