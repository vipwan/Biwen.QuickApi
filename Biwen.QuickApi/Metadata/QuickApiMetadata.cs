namespace Biwen.QuickApi.Metadata
{
    /// <summary>
    /// Metadata for QuickApi
    /// </summary>
    public class QuickApiMetadata(Type? quickApiType, QuickApiAttribute? quickApiAttribute = null)
    {

        //public QuickApiMetadata(
        //    Type? quickApiType,
        //    IEndpointSummaryMetadata endpointSummaryMetadata,
        //    IEndpointDescriptionMetadata? endpointDescriptionMetadata)
        //{
        //    QuickApiType = quickApiType;
        //    EndpointSummaryMetadata = endpointSummaryMetadata;
        //    EndpointDescriptionMetadata = endpointDescriptionMetadata;
        //}

        public Type? QuickApiType { get; set; } = quickApiType;

        /// <summary>
        /// 源代码生成器的metadata可能该项为null.
        /// </summary>
        public QuickApiAttribute? QuickApiAttribute { get; set; } = quickApiAttribute;

        //public IEndpointSummaryMetadata? EndpointSummaryMetadata { get; set; }

        //public IEndpointDescriptionMetadata? EndpointDescriptionMetadata { get; set; }
    }
}