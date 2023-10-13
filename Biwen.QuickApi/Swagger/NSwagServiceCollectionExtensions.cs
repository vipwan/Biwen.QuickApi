
using Biwen.QuickApi.Swagger;
using Newtonsoft.Json;
using NSwag.Generation.AspNetCore;

namespace Biwen.QuickApi
{
    public static class NSwagServiceCollectionExtensions
    {

        /// <summary>
        /// 添加 QuickApi Nswag Service
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuickApiDocument(this IServiceCollection serviceCollection, Action<AspNetCoreOpenApiDocumentGeneratorSettings> configure)
        {
            serviceCollection.AddEndpointsApiExplorer();

            return serviceCollection.AddOpenApiDocument(delegate (AspNetCoreOpenApiDocumentGeneratorSettings settings, IServiceProvider services)
            {
                settings.OperationProcessors.Add(new QuickApiOperationProcessor());
                settings.SchemaProcessors.Add(new QuickApiSchemaProcessor());
                settings.SerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new QuickApiContractResolver()
                };
                configure?.Invoke(settings);
            });
        }
    }
}