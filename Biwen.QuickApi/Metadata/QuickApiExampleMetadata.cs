namespace Biwen.QuickApi.Metadata
{
    /// <summary>
    /// Example Metadata for QuickApi
    /// </summary>
    public class QuickApiExampleMetadata(ICollection<object?> examples)
    {
        public ICollection<object?> Examples { get; set; } = examples;
    }
}