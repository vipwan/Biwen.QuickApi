// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:54:08 FileStorageExtensions.cs

using Biwen.QuickApi.Serializer;
using System.Text;

namespace Biwen.QuickApi.Storage
{
    /// <summary>
    /// 文件存储扩展方法
    /// </summary>
    public static class FileStorageExtensions
    {
        /// <summary>
        /// 存储文件,使用内部序列化器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<bool> SaveObjectAsync<T>(this IFileStorage storage, string path, T data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);

            var bytes = storage.Serializer.SerializeToBytes(data);
            return storage.SaveFileAsync(path, new MemoryStream(bytes), cancellationToken);
        }

        public static async Task<T?> GetObjectAsync<T>(this IFileStorage storage, string path, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);

#pragma warning disable CS0618 // Type or member is obsolete
            using var stream = await storage.GetFileStreamAsync(path, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
            if (stream != null)
                return storage.Serializer.Deserialize<T>(stream);

            return default;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public static async Task DeleteFilesAsync(this IFileStorage storage, IEnumerable<FileSpec> files)
        {
            ArgumentNullException.ThrowIfNull(files);

            foreach (var file in files)
                await storage.DeleteFileAsync(file.Path);
        }

        /// <summary>
        /// 获取文本文件内容
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<string?> GetFileContentsAsync(this IFileStorage storage, string path)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);

#pragma warning disable CS0618 // Type or member is obsolete
            using var stream = await storage.GetFileStreamAsync(path);
#pragma warning restore CS0618 // Type or member is obsolete
            if (stream != null)
                return await new StreamReader(stream).ReadToEndAsync();

            return null;
        }

        public static async Task<byte[]> GetFileContentsRawAsync(this IFileStorage storage, string path)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);

#pragma warning disable CS0618 // Type or member is obsolete
            using var stream = await storage.GetFileStreamAsync(path);
#pragma warning restore CS0618 // Type or member is obsolete
            if (stream == null)
                return null!;

            var buffer = new byte[16 * 1024];
            using var ms = new MemoryStream();
            int read;
            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
                await ms.WriteAsync(buffer.AsMemory(0, read));
            }

            return ms.ToArray();
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="path"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static Task<bool> SaveFileAsync(this IFileStorage storage, string path, string contents)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);

            return storage.SaveFileAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(contents ?? String.Empty)));
        }

        public static async Task<IReadOnlyCollection<FileSpec>> GetFileListAsync(this IFileStorage storage, string? searchPattern = null, int? limit = null, CancellationToken cancellationToken = default)
        {
            var files = new List<FileSpec>();
            limit ??= int.MaxValue;
            var result = await storage.GetPagedFileListAsync(limit.Value, searchPattern, cancellationToken);
            do
            {
                files.AddRange(result.Files);
            } while (result.HasMore && files.Count < limit.Value && await result.NextPageAsync());

            return files;
        }
    }

}
