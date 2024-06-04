using Microsoft.AspNetCore.OpenApi;

namespace Biwen.QuickApi.OpenApi
{
    public static class ServiceRegistration
    {
        /// <summary>
        /// AddOpenApi扩展,支持只包含QuickApiMetadata的Api
        /// </summary>
        /// <param name="services"></param>
        /// <param name="documentName"></param>
        /// <param name="configureOptions"></param>
        /// <param name="onlyQuickApi"></param>
        /// <returns></returns>
        public static IServiceCollection AddOpenApi(this IServiceCollection services, string documentName, Action<OpenApiOptions>? configureOptions = null
            , bool onlyQuickApi = false)
        {
            configureOptions = configureOptions ?? (_ => { });
            services.Configure<OpenApiOptions>(documentName, options =>
            {
                options.UseTransformer<BearerSecuritySchemeTransformer>();
                if (onlyQuickApi)
                {
                    options.ShouldInclude = desc =>
                    {
                        //return true;
                        //如果包含QuickApiMetadata返回True,否则返回False:
                        return desc.ActionDescriptor.EndpointMetadata.OfType<QuickApiMetadata>().Any();
                    };
                }
                else
                {
                    options.ShouldInclude = _ => true;
                }
                configureOptions(options);
            });

            //AddOpenApi
            OpenApiServiceCollectionExtensions.AddOpenApi(services, documentName, configureOptions);
            return services;
        }
    }
}