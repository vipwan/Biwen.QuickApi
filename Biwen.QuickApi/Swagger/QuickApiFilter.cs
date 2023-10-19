namespace Biwen.QuickApi.Swagger
{
    using NSwag.Generation.AspNetCore;
    using NSwag.Generation.Processors.Contexts;
    using NSwag.Generation.Processors;
    using NJsonSchema.Generation;
    using System;

    /// <summary>
    /// 筛选器. 用于过滤掉所有非QuickApi的Endpoint
    /// </summary>
    internal sealed class QuickApiFilter : IOperationProcessor
    {
        public bool Process(OperationProcessorContext ctx)
        {
            var metaData = ((AspNetCoreOperationProcessorContext)ctx).ApiDescription.ActionDescriptor.EndpointMetadata;
            return metaData.OfType<QuickApiMetadata>().Any();
        }
    }



}