// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:54:27 PathHelper.cs

namespace Biwen.QuickApi.Storage;

internal static class PathHelper
{
    private const string DATA_DIRECTORY = "|DataDirectory|";

    public static string? ExpandPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        path = path.Replace('/', Path.DirectorySeparatorChar);
        path = path.Replace('\\', Path.DirectorySeparatorChar);

        if (!path.StartsWith(DATA_DIRECTORY, StringComparison.OrdinalIgnoreCase))
            return Path.GetFullPath(path);

        string? dataDirectory = GetDataDirectory();
        int length = DATA_DIRECTORY.Length;
        if (path.Length <= length)
            return dataDirectory;

        string relativePath = path.Substring(length);
        char c = relativePath[0];

        if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
            relativePath = relativePath.Substring(1);

        string fullPath = Path.Combine(dataDirectory ?? String.Empty, relativePath);
        fullPath = Path.GetFullPath(fullPath);

        return fullPath;
    }

    public static string? GetDataDirectory()
    {
        try
        {
            string? dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(dataDirectory))
                dataDirectory = AppContext.BaseDirectory;

            if (!string.IsNullOrEmpty(dataDirectory))
                return Path.GetFullPath(dataDirectory);
        }
        catch (Exception)
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// NormalizePath
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string NormalizePath(this string path)
    {
        if (String.IsNullOrEmpty(path))
            return path;

        if (Path.DirectorySeparatorChar == '\\')
            path = path.Replace('/', Path.DirectorySeparatorChar);
        else if (Path.DirectorySeparatorChar == '/')
            path = path.Replace('\\', Path.DirectorySeparatorChar);

        return path;
    }


}
