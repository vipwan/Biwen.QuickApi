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

    }
}