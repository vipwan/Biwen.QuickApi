﻿
using Biwen.QuickApi.Swagger;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors.Security;
using System.Net;

namespace Biwen.QuickApi
{
    public static class NSwagExtensions
    {

        /// <summary>
        /// add NSwag doc services for QuickApi.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configure"></param>
        /// <param name="securityOptions"></param>
        /// <param name="onlyQuickApi">是否只对QuickApi生成文档,默认:False</param>
        /// <returns></returns>
        public static IServiceCollection AddQuickApiDocument(
            this IServiceCollection serviceCollection,
            Action<AspNetCoreOpenApiDocumentGeneratorSettings> configure,
            SecurityOptions? securityOptions = null,
            bool onlyQuickApi = false)
        {
            //配置JsonSerializerOptions的枚举转换器
            //serviceCollection.Configure<JsonSerializerOptions>(x =>
            //{
            //    x.Converters.Add(new JsonStringEnumConverter());
            //});

#if NET8_0_OR_GREATER
            // .NET 8.0 or above use NSawg 14 set ContractResolver
            serviceCollection.Configure<JsonSerializerSettings>(settings =>
            {
                settings.ContractResolver = new QuickApiContractResolver();
            });
#endif
            serviceCollection.AddEndpointsApiExplorer();
            serviceCollection.AddOpenApiDocument(delegate (AspNetCoreOpenApiDocumentGeneratorSettings settings, IServiceProvider services)
            {
                settings.OperationProcessors.Add(new QuickApiOperationProcessor());
                // .NET 7.0 or below use NSawg 13
#if !NET8_0_OR_GREATER
                settings.SchemaProcessors.Add(new QuickApiSchemaProcessor());
                settings.SchemaProcessors.Add(new QuickApiValidationSchemaProcessor());
                settings.SerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new QuickApiContractResolver()
                };
                // EnumDescription
                settings.GenerateEnumMappingDescription = true;
                // 隐藏AllOf的Scheme
                settings.FlattenInheritanceHierarchy = true;
#endif
                // .NET 8.0 or above use NSawg 14+
#if NET8_0_OR_GREATER
                settings.SchemaSettings.SchemaProcessors.Add(new QuickApiSchemaProcessor());
                settings.SchemaSettings.SchemaProcessors.Add(new QuickApiValidationSchemaProcessor());
                // EnumDescription
                settings.SchemaSettings.GenerateEnumMappingDescription = true;
                // 隐藏AllOf的Scheme
                settings.SchemaSettings.FlattenInheritanceHierarchy = true;
#endif

                if (securityOptions?.EnlableSecurityProcessor is true)
                {
                    settings.OperationProcessors.Add(new QuickApiOperationSecurityProcessor(securityOptions));
                    settings.DocumentProcessors.Add(
                        new SecurityDefinitionAppender(securityOptions.SecretScheme,
                        new OpenApiSecurityScheme
                        {
                            Type = OpenApiSecuritySchemeType.Http,
                            Name = nameof(Authorization),
                            In = OpenApiSecurityApiKeyLocation.Header,
                            Description = "Copy this into the value field: Bearer {token}",
                            BearerFormat = "jwt",
                            Scheme = "bearer"
                        }));
                }
                if (onlyQuickApi)
                {
                    //排除非QuickApi的接口
                    settings.OperationProcessors.Insert(0, new QuickApiFilter());
                }

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
#if !NET8_0_OR_GREATER
        public static IApplicationBuilder UseQuickApiSwagger(this IApplicationBuilder app, Action<OpenApiDocumentMiddlewareSettings>? config = null, Action<SwaggerUi3Settings>? uiConfig = null)
        {
            app.UseOpenApi(config);
            app.UseSwaggerUi3(uiConfig);
            return app;
        }
#endif
#if NET8_0_OR_GREATER
        public static IApplicationBuilder UseQuickApiSwagger(this IApplicationBuilder app, Action<OpenApiDocumentMiddlewareSettings>? config = null, Action<SwaggerUiSettings>? uiConfig = null)
        {
            app.UseOpenApi(config);
            app.UseSwaggerUi(uiConfig);
            return app;
        }
#endif

    }
}