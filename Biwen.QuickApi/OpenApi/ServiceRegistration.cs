using Microsoft.AspNetCore.OpenApi;

namespace Biwen.QuickApi.OpenApi
{
    [SuppressType]
    public static class ServiceRegistration
    {
        /// <summary>
        /// AddOpenApi扩展,支持只包含QuickApiMetadata的Api
        /// </summary>
        /// <param name="services"></param>
        /// <param name="documentName"></param>
        /// <param name="onlyQuickApi"></param>
        /// <param name="groups">需要包含的分组.null表示全量</param>
        /// <returns></returns>
        public static IServiceCollection AddOpenApi(
            this IServiceCollection services,
            string documentName = "v1",
            bool onlyQuickApi = false,
            string[]? groups = null)
        {
            Action<OpenApiOptions> configureOptions = options =>
            {
                options.UseTransformer<BearerSecuritySchemeTransformer>();
                if (onlyQuickApi)
                {
                    options.ShouldInclude = desc =>
                    {
                        //如果包含QuickApiMetadata返回True,否则返回False:
                        return
                        (desc.ActionDescriptor.EndpointMetadata.OfType<QuickApiMetadata>().Any() ||
                        desc.ActionDescriptor.EndpointMetadata.OfType<QuickApiEndpointMetadata>().Any())
                        &&
                        (groups?.Contains(desc.GroupName) is true || groups is null);
                    };
                }
                else
                {
                    options.ShouldInclude = desc =>
                    (groups?.Contains(desc.GroupName) is true || groups is null);
                }
            };

            //AddOpenApi
            OpenApiServiceCollectionExtensions.AddOpenApi(services, documentName, configureOptions);
            return services;
        }

    }
}