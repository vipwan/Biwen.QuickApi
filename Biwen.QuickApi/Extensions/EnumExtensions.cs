namespace Biwen.QuickApi
{
    [SuppressType]
    internal static class EnumExtensions
    {
        /// <summary>
        /// 拆分枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IEnumerable<T> SplitEnum<T>(this T e) where T : Enum
        {
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                if ((Convert.ToInt32(item) & Convert.ToInt32(e)) > 0)
                {
                    yield return item;
                }
            }
        }
    }
}