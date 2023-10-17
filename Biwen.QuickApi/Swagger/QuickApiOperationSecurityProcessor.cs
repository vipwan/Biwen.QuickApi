namespace Biwen.QuickApi.Swagger
{
    using Biwen.QuickApi.Metadata;
    using Microsoft.AspNetCore.Authorization;
    using NSwag;
    using NSwag.Generation.AspNetCore;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Contexts;

    /// <summary>
    /// QuickApiOperationSecurityProcessor
    /// </summary>
    internal sealed class QuickApiOperationSecurityProcessor : IOperationProcessor
    {

        private readonly SecurityOptions? _securityOptions;

        public QuickApiOperationSecurityProcessor(SecurityOptions? options)
        {
            _securityOptions = options;
        }

        public bool Process(OperationProcessorContext context)
        {
            //如果没有启用安全处理器
            if (_securityOptions == null || !_securityOptions.EnlableSecurityProcessor)
                return true;

            //如果没有设置Scheme
            if (string.IsNullOrEmpty(_securityOptions.SecretScheme))
                return true;

            var quikcApiMeta = ((AspNetCoreOperationProcessorContext)context).ApiDescription.ActionDescriptor.EndpointMetadata;

            if (quikcApiMeta is null)
                return true;

            var epDef = quikcApiMeta.OfType<QuickApiMetadata>().SingleOrDefault();

            if (epDef is null)
                return true;

            var apiAttribute = epDef.QuickApiType!.GetCustomAttribute<QuickApiAttribute>();
            if (apiAttribute is null)
                return true;

            var policys = new List<string>();
            if (!string.IsNullOrEmpty(apiAttribute.Policy))
            {
                policys.Add(apiAttribute.Policy);
            }
            var authAttributes = epDef.QuickApiType!.GetCustomAttributes<AuthorizeAttribute>();
            foreach (var authAttribute in authAttributes)
            {
                if (authAttribute?.Policy != null)
                {
                    policys.Add(authAttribute.Policy);
                }
            }
            var authMetadatas = quikcApiMeta.OfType<AuthorizeAttribute>();
            foreach (var authMeta in authMetadatas)
            {
                if (authMeta?.Policy != null)
                {
                    policys.Add(authMeta.Policy);
                }
            }

            if (policys.Count == 0)
                return true;

            (context.OperationDescription.Operation.Security ??= new List<OpenApiSecurityRequirement>()).Add(
            new OpenApiSecurityRequirement
            {
                {
                    _securityOptions.SecretScheme,
                    policys.Distinct()
                }
            });

            return true;
        }
    }

    /// <summary>
    /// SecurityOptions default is true & "Bearer"
    /// </summary>
    /// <param name="EnlableSecurityProcessor"></param>
    /// <param name="Scheme"></param>
    public record SecurityOptions(bool EnlableSecurityProcessor = true, string? SecretScheme = "Bearer");

}