
using Microsoft.AspNetCore.Http.Metadata;

namespace Biwen.QuickApi
{
    /// <summary>
    /// Metadata for QuickApi
    /// </summary>
    public class QuickApiMetadata
    {

        public QuickApiMetadata(Type? quickApiType)
        {
            QuickApiType = quickApiType;
        }

        public QuickApiMetadata(
            Type? quickApiType,
            IEndpointSummaryMetadata endpointSummaryMetadata,
            IEndpointDescriptionMetadata? endpointDescriptionMetadata)
        {
            QuickApiType = quickApiType;
            EndpointSummaryMetadata = endpointSummaryMetadata;
            EndpointDescriptionMetadata = endpointDescriptionMetadata;
        }

        public Type? QuickApiType { get; set; }

        public IEndpointSummaryMetadata? EndpointSummaryMetadata { get; set; }

        public IEndpointDescriptionMetadata? EndpointDescriptionMetadata { get; set; }
    }

    /// <summary>
    /// Example Metadata for QuickApi
    /// </summary>
    public class QuickApiExampleMetadata
    {
        public QuickApiExampleMetadata(object? example)
        {
            Example = example;
        }
        public object? Example { get; set; }
    }
}