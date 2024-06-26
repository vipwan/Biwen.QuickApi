using Biwen.QuickApi.Serializer;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Biwen.QuickApi.Storage
{
#pragma warning disable GEN031 // 使用[AutoGen]自动生成
    public interface IFileStorage : IDisposable
    {
        /// <summary>
        /// 序列化器
        /// </summary>
        ISerializer Serializer { get; }


        [Obsolete($"Use {nameof(GetFileStreamAsync)} with {nameof(StreamMode)} instead to define read or write behaviour of stream")]
        Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a file stream in the specified mode
        /// </summary>
        /// <param name="path">Path to the file in the file storage</param>
        /// <param name="streamMode">What the stream is used for</param>
        /// <param name="cancellationToken">Token to cancel</param>
        /// <returns>Stream in the specified mode</returns>
        Task<Stream> GetFileStreamAsync(string path, StreamMode streamMode, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<FileSpec?> GetFileInfoAsync(string path);
        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string path);
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default);
        /// <summary>
        /// 重命名文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default);
        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="targetPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default);
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default);
        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<int> DeleteFilesAsync(string? searchPattern = null, CancellationToken cancellation = default);
        /// <summary>
        /// 获取分页文件信息
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="searchPattern"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PagedFileListResult> GetPagedFileListAsync(int pageSize = 100, string? searchPattern = null, CancellationToken cancellationToken = default);
    }
#pragma warning restore GEN031 // 使用[AutoGen]自动生成


    public interface IHasNextPageFunc
    {
        Func<PagedFileListResult, Task<NextPageResult>> NextPageFunc { get; set; }
    }

    public class NextPageResult
    {
        public bool Success { get; set; }
        public bool HasMore { get; set; }
        public IReadOnlyCollection<FileSpec> Files { get; set; } = null!;
        public Func<PagedFileListResult, Task<NextPageResult>>? NextPageFunc { get; set; }
    }

    public class PagedFileListResult : IHasNextPageFunc
    {
        private static readonly IReadOnlyCollection<FileSpec> _empty = new ReadOnlyCollection<FileSpec>(Array.Empty<FileSpec>());
        public static readonly PagedFileListResult Empty = new(_empty);

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public PagedFileListResult(IReadOnlyCollection<FileSpec> files)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            Files = files;
            HasMore = false;
            ((IHasNextPageFunc)this).NextPageFunc = null!;
        }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public PagedFileListResult(IReadOnlyCollection<FileSpec> files, bool hasMore, Func<PagedFileListResult, Task<NextPageResult>> nextPageFunc)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            Files = files;
            HasMore = hasMore;
            ((IHasNextPageFunc)this).NextPageFunc = nextPageFunc;
        }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public PagedFileListResult(Func<PagedFileListResult, Task<NextPageResult>> nextPageFunc)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            ((IHasNextPageFunc)this).NextPageFunc = nextPageFunc;
        }

        public IReadOnlyCollection<FileSpec> Files { get; private set; }
        public bool HasMore { get; private set; }
        protected IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
        Func<PagedFileListResult, Task<NextPageResult>> IHasNextPageFunc.NextPageFunc { get; set; }

        public async Task<bool> NextPageAsync()
        {
            if (((IHasNextPageFunc)this).NextPageFunc == null)
                return false;

            var result = await ((IHasNextPageFunc)this).NextPageFunc(this);
            if (result.Success)
            {
                Files = result.Files;
                HasMore = result.HasMore;
                ((IHasNextPageFunc)this).NextPageFunc = result.NextPageFunc!;
            }
            else
            {
                Files = _empty;
                HasMore = false;
                ((IHasNextPageFunc)this).NextPageFunc = null!;
            }

            return result.Success;
        }
    }

    /// <summary>
    /// 文件信息
    /// </summary>
    [DebuggerDisplay("Path = {Path}, Created = {Created}, Modified = {Modified}, Size = {Size} bytes")]
    public class FileSpec
    {
        public string Path { get; set; } = null!;
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        /// <summary>
        /// In Bytes
        /// </summary>
        public long Size { get; set; }
        // TODO: Add metadata object for custom properties
    }


}
