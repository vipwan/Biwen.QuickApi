namespace Biwen.QuickApi.Metadata
{
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