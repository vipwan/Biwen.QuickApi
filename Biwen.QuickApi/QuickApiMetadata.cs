
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
            //QuickApiAttribute = quickApiAttribute;
        }

        public Type? QuickApiType { get; set; }
        //public QuickApiAttribute? QuickApiAttribute { get; set; }
    }
}