using System.Collections.Concurrent;

namespace Biwen.QuickApi
{
    /// <summary>
    /// 内部缓存
    /// </summary>
    internal static class Caching
    {
        /// <summary>
        /// 缓存T是否有DataAnnotation
        /// </summary>
        public static readonly ConcurrentDictionary<string, bool> TAnnotationAttrs = new();


        /// <summary>
        /// 缓存订阅者的Metadata
        /// </summary>
        public static readonly ConcurrentDictionary<Type, object> SubscriberMetadatas = new();

        /// <summary>
        /// ReqType是否是:multipart/form-data
        /// </summary>
        public static readonly ConcurrentDictionary<Type, bool> ReqTypeIsFormdatas = new();

    }
}