namespace Biwen.QuickApi.SourceGenerator
{
    using System;
    internal static class StringExtentions
    {
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            if (string.IsNullOrEmpty(str)) { return str; }
            if (str.Length == 1) { return str.ToLower(); }
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }

        public static string ToPascalCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) { return str; }
            if (str.Length == 1) { return str.ToUpper(); }
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }


        /// <summary>
        /// 删除""
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToRaw(this string str)
        {
            if (string.IsNullOrEmpty(str)) { return str; }
            if (str.StartsWith("\"", StringComparison.Ordinal) && str.EndsWith("\"", StringComparison.Ordinal))
            {
                return str.Substring(1, str.Length - 2);
            }
            return str;
        }

    }
}