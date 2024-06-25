using Biwen.QuickApi.Serializer;
using System.Text;

namespace Biwen.QuickApi.Storage
{

    public static class FileStorageExtensions
    {

        public static Task<bool> SaveObjectAsync<T>(this IFileStorage storage, string path, T data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var bytes = storage.Serializer.SerializeToBytes(data);
            return storage.SaveFileAsync(path, new MemoryStream(bytes), cancellationToken);
        }

        public static async Task<T?> GetObjectAsync<T>(this IFileStorage storage, string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

#pragma warning disable CS0618 // Type or member is obsolete
            using var stream = await storage.GetFileStreamAsync(path, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
            if (stream != null)
                return storage.Serializer.Deserialize<T>(stream);

            return default;
        }

        public static async Task DeleteFilesAsync(this IFileStorage storage, IEnumerable<FileSpec> files)
        {
            ArgumentNullException.ThrowIfNull(files);

            foreach (var file in files)
                await storage.DeleteFileAsync(file.Path);
        }

        public static async Task<string?> GetFileContentsAsync(this IFileStorage storage, string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

#pragma warning disable CS0618 // Type or member is obsolete
            using var stream = await storage.GetFileStreamAsync(path);
#pragma warning restore CS0618 // Type or member is obsolete
            if (stream != null)
                return await new StreamReader(stream).ReadToEndAsync();

            return null;
        }

        public static async Task<byte[]> GetFileContentsRawAsync(this IFileStorage storage, string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

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

        public static Task<bool> SaveFileAsync(this IFileStorage storage, string path, string contents)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

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
