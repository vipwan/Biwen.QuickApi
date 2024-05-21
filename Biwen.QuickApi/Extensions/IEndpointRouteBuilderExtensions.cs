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
            return app.MapMethods(pattern, verbs.Select(x => x.ToString()), T.Handler());
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
