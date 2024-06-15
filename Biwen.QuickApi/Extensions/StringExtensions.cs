using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
namespace System;

/// <summary>
/// 字符串扩展类
/// </summary>
public static class StringExtensions
{

    /// <summary>
    /// 判定是否为字母a-z,A-Z
    /// </summary>
    public static bool IsLetter(this char c)
    {
        return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
    }

    /// <summary>
    /// 判定是否为空格
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static bool IsSpace(this char c)
    {
        return c is '\r' or '\n' or '\t' or '\f' or ' ';
    }

    /// <summary>
    /// 移除字符串中的html标签
    /// </summary>
    /// <param name="html"></param>
    /// <param name="htmlDecode"></param>
    /// <returns></returns>
    public static string RemoveTags(this string html, bool htmlDecode = false)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        var result = new char[html.Length];

        var cursor = 0;
        var inside = false;
        for (var i = 0; i < html.Length; i++)
        {
            var current = html[i];

            switch (current)
            {
                case '<':
                    inside = true;
                    continue;
                case '>':
                    inside = false;
                    continue;
            }

            if (!inside)
            {
                result[cursor++] = current;
            }
        }

        var stringResult = new string(result, 0, cursor);

        if (htmlDecode)
        {
            stringResult = WebUtility.HtmlDecode(stringResult);
        }

        return stringResult;
    }

    /// <summary>
    /// 将字符串转换为帕斯卡命名法
    /// </summary>
    public static string ToPascalCase(this string attribute, char upperAfterDelimiter = ' ')
    {
        attribute = attribute.Trim();

        var delimitersCount = 0;

        for (var i = 0; i < attribute.Length; i++)
        {
            if (attribute[i] == upperAfterDelimiter)
            {
                delimitersCount++;
            }
        }

        var result = string.Create(attribute.Length - delimitersCount, new { attribute, upperAfterDelimiter }, (buffer, state) =>
        {
            var nextIsUpper = true;
            var k = 0;

            for (var i = 0; i < state.attribute.Length; i++)
            {
                var c = state.attribute[i];

                if (c == state.upperAfterDelimiter)
                {
                    nextIsUpper = true;
                    continue;
                }

                if (nextIsUpper)
                {
                    buffer[k] = char.ToUpperInvariant(c);
                }
                else
                {
                    buffer[k] = c;
                }

                nextIsUpper = false;

                k++;
            }
        });

        return result;
    }

    /// <summary>
    /// 将字符串中的空格转换为驼峰
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static string ToCamalCase(this string attribute)
    {
        if (string.IsNullOrEmpty(attribute))
        {
            return string.Empty;
        }

        //去除空格
        attribute = attribute.Trim();

        var result = new char[attribute.Length];

        var cursor = 0;
        var nextIsUpper = false;
        for (var i = 0; i < attribute.Length; i++)
        {
            var current = attribute[i];

            if (current == ' ')
            {
                nextIsUpper = true;
                continue;
            }

            if (nextIsUpper)
            {
                result[cursor++] = char.ToUpperInvariant(current);
            }
            else
            {
                result[cursor++] = current;
            }

            nextIsUpper = false;
        }

        return new string(result, 0, cursor);

    }


    /// <summary>
    /// 字符串替换
    /// </summary>
    /// <param name="original"></param>
    /// <param name="replacements"></param>
    /// <returns></returns>
    public static string ReplaceAll(this string original, IDictionary<string, string> replacements)
    {
        var pattern = $"{string.Join("|", replacements.Keys)}";
        return Regex.Replace(original, pattern, match => replacements[match.Value]);
    }

    /// <summary>
    /// 判定是否为json字符串
    /// </summary>
    public static bool IsJson(this string json, JsonDocumentOptions jsonDocumentOptions = default)
    {
        try
        {
            JsonNode.Parse(json, null, jsonDocumentOptions);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// MD5加密
    /// </summary>
    /// <param name="text">需要加密的文本</param>
    /// <param name="uppercase">生成大小写,默认:小写</param>
    /// <returns></returns>
    public static string ToMD5(this string text, bool uppercase = false)
    {
        var inputBytes = Encoding.ASCII.GetBytes(text);
        var hash = MD5.HashData(inputBytes);

        var sb = new StringBuilder();
        for (var i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("X2"));
        }
        return uppercase ? sb.ToString() : sb.ToString().ToLower();
    }

    /// <summary>
    /// SHA1加密
    /// </summary>
    /// <param name="text">需要加密的文本</param>
    /// <param name="uppercase">生成大小写,默认:小写</param>
    /// <returns></returns>
    public static string ToSHA1(this string text, bool uppercase = false)
    {
        var inputBytes = Encoding.ASCII.GetBytes(text);
        var hash = SHA1.HashData(inputBytes);

        var sb = new StringBuilder();
        for (var i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("X2"));
        }
        return uppercase ? sb.ToString() : sb.ToString().ToLower();
    }


}