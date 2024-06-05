using Microsoft.AspNetCore.Builder;
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
        public static RouteHandlerBuilder MapMethods<T>(
            this IEndpointRouteBuilder app,
            [StringSyntax("Route")] string pattern,
            bool excludeFromDescription = false) where T : class, IQuickEndpoint
        {
            var verbs = T.Verbs.SplitEnum();
            var builder = app.MapMethods(pattern, verbs.Select(x => x.ToString()), T.Handler);
            //获取T的所有Attribute:
            var attrs = typeof(T).GetCustomAttributes(true);
            //将所有的Attribute添加到metadatas中
            builder.WithMetadata(attrs);
            //QuickApiEndpointMetadata
            builder.WithMetadata(new QuickApiEndpointMetadata());

            //OpenApiMetadataAttribute
            var openApiMetadata = attrs.OfType<OpenApiMetadataAttribute>().FirstOrDefault();
            if (openApiMetadata is not null)
            {
                if (openApiMetadata.Tags.Length > 0)
                {
                    builder.WithTags(openApiMetadata.Tags);
                }
                if (!string.IsNullOrEmpty(openApiMetadata.Summary))
                {
                    builder.WithSummary(openApiMetadata.Summary);
                }
                if (!string.IsNullOrEmpty(openApiMetadata.Description))
                {
                    builder.WithDescription(openApiMetadata.Description);
                }

                //兼容性问题,Verbs数量>1将不会添加OperationId等信息
                if (verbs.Count == 1)
                {
                    builder.WithOpenApi(operation => new(operation)
                    {
                        Deprecated = openApiMetadata.IsDeprecated,
                        OperationId = openApiMetadata.OperationId,
                        //参数的备注和example等:
                        //Parameters = GetParameters(T.Handler)
                    });
                }
            }

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
                        if (arg is null) continue;
                        var argType = arg?.GetType();
                        if (argType is null || !argType.IsClass) continue;
                        var methodName = nameof(BaseRequest<EmptyRequest>.Validate);
                        //使用反射验证参数:
                        if (argType.GetMethod(methodName) is not null)
                        {
                            try
                            {
                                //验证DTO
                                if (((dynamic)arg!).Validate() is ValidationResult { IsValid: false } vresult)
                                {
                                    return TypedResults.ValidationProblem(vresult.ToDictionary());
                                }
                            }
                            catch
                            {
                                //特殊情况,不处理
                                continue;
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
        public static RouteGroupBuilder MapGroup(
            this IEndpointRouteBuilder endpoints,
            [StringSyntax("Route")] string prefix,
            Action<IEndpointRouteBuilder> action)
        {
            var group = endpoints.MapGroup(prefix);
            action(group);
            return group;
        }
    }
}
