
using Biwen.QuickApi.Swagger;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using NSwag.AspNetCore;
using NSwag.Generation.AspNetCore;

namespace Biwen.QuickApi
{
    public static class NSwagExtensions
    {

        /// <summary>
        /// add NSwag doc services for QuickApi.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuickApiDocument(this IServiceCollection serviceCollection, Action<AspNetCoreOpenApiDocumentGeneratorSettings> configure)
        {
            serviceCollection.AddEndpointsApiExplorer();
            serviceCollection.AddOpenApiDocument(delegate (AspNetCoreOpenApiDocumentGeneratorSettings settings, IServiceProvider services)
            {
                settings.OperationProcessors.Add(new QuickApiOperationProcessor());
                settings.SchemaProcessors.Add(new QuickApiSchemaProcessor());
                settings.SerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new QuickApiContractResolver()
                };
                configure?.Invoke(settings);
            });

            return serviceCollection;
        }

        /// <summary>
        /// enables the open-api/swagger middleware for QuickApi.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="config"></param>
        /// <param name="uiConfig"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseQuickApiSwagger(this IApplicationBuilder app, Action<OpenApiDocumentMiddlewareSettings>? config = null, Action<SwaggerUi3Settings>? uiConfig = null)
        {
            app.UseOpenApi(config);
            app.UseSwaggerUi3(uiConfig);
            return app;
        }
    }
}