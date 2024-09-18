// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:54:23 LocalFileStorage.cs

using Biwen.QuickApi.Serializer;

namespace Biwen.QuickApi.Storage;

/// <summary>
/// 本地存储
/// </summary>
public class LocalFileStorage : IFileStorage
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="StoragePath">存储文件夹路径</param>
    /// <param name="serviceProvider">sp</param>
    /// <exception cref="QuickApiExcetion"></exception>
    public LocalFileStorage(IServiceProvider serviceProvider, string StoragePath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(StoragePath);

        Serializer = serviceProvider.GetService<ISerializer>() ?? DefaultSerializer.Instance;
        _logger = serviceProvider.GetRequiredService<ILogger<LocalFileStorage>>();

        var folder = PathHelper.ExpandPath(StoragePath)! ?? throw new QuickApiExcetion("文件夹格式错误!");

        if (!Path.IsPathRooted(folder))
            folder = Path.GetFullPath(folder);

        char lastCharacter = folder[folder.Length - 1];
        if (!lastCharacter.Equals(Path.DirectorySeparatorChar) && !lastCharacter.Equals(Path.AltDirectorySeparatorChar))
            folder += Path.DirectorySeparatorChar;

        Folder = folder;

        _logger.LogDebug("Creating {Directory} directory", folder);

        lock (_lock)
            Directory.CreateDirectory(folder);

    }

    /// <summary>
    /// 文件夹路径
    /// </summary>
    public string Folder { get; private set; }

    private static object _lock = new();

    public ISerializer Serializer { get; }

    private readonly ILogger<LocalFileStorage> _logger;

    #region helper

    private void EnsureDirectory(string normalizedPath)
    {
        var directory = Path.GetDirectoryName(normalizedPath);
        if (directory == null)
            return;

        _logger.LogTrace("Ensuring director: {Directory}", directory);
        Directory.CreateDirectory(directory);
    }

    /// <summary>
    /// 获取分页文件列表
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    private NextPageResult GetFiles(string searchPattern, int page, int pageSize)
    {
        var list = new List<FileSpec>();
        int pagingLimit = pageSize;
        int skip = (page - 1) * pagingLimit;
        if (pagingLimit < Int32.MaxValue)
            pagingLimit++;

        foreach (string path in Directory.EnumerateFiles(Folder, searchPattern, SearchOption.AllDirectories).Skip(skip).Take(pagingLimit))
        {
            var info = new FileInfo(path);
            if (!info.Exists)
                continue;

            list.Add(new FileSpec
            {
                Path = info.FullName.Replace(Folder, String.Empty),
                Created = info.CreationTimeUtc,
                Modified = info.LastWriteTimeUtc,
                Size = info.Length
            });
        }

        bool hasMore = false;
        if (list.Count == pagingLimit)
        {
            hasMore = true;
            list.RemoveAt(pagingLimit - 1);
        }

        return new NextPageResult
        {
            Success = true,
            HasMore = hasMore,
            Files = list,
            NextPageFunc = hasMore ? _ => Task.FromResult(GetFiles(searchPattern, page + 1, pageSize)) : null
        };
    }

    #endregion

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="targetPath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNullOrEmpty(targetPath);

        string normalizedPath = path.NormalizePath();
        string normalizedTargetPath = targetPath.NormalizePath();
        _logger.LogDebug("Copying {Path} to {TargetPath}", normalizedPath, normalizedTargetPath);

        try
        {
            lock (_lock)
            {
                var directory = Path.GetDirectoryName(normalizedTargetPath);
                if (directory != null)
                {
                    _logger.LogDebug("Creating {Directory} directory", directory);
                    Directory.CreateDirectory(Path.Combine(Folder, directory));
                }

                File.Copy(Path.Combine(Folder, normalizedPath), Path.Combine(Folder, normalizedTargetPath));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying {Path} to {TargetPath}: {Message}", normalizedPath, normalizedTargetPath, ex.Message);
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

    /// <summary>
    /// 删除指定文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);

        string normalizedPath = path.NormalizePath();
        _logger.LogTrace("Deleting {Path}", normalizedPath);

        try
        {
            File.Delete(Path.Combine(Folder, normalizedPath));
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            _logger.LogError(ex, "Unable to delete {Path}: {Message}", normalizedPath, ex.Message);
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

    /// <summary>
    /// 根据搜索模式删除文件
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public Task<int> DeleteFilesAsync(string? searchPattern = null, CancellationToken cancellation = default)
    {
        int count = 0;

        if (string.IsNullOrEmpty(searchPattern) || searchPattern == "*")
        {
            if (Directory.Exists(Folder))
            {
                _logger.LogDebug("Deleting {Directory} directory", Folder);
                count += Directory.EnumerateFiles(Folder, "*,*", SearchOption.AllDirectories).Count();
                Directory.Delete(Folder, true);
                _logger.LogTrace("Finished deleting {Directory} directory with {FileCount} files", Folder, count);
            }

            return Task.FromResult(count);
        }

        searchPattern = searchPattern.NormalizePath();
        string path = Path.Combine(Folder, searchPattern);
        if (path[path.Length - 1] == Path.DirectorySeparatorChar || path.EndsWith(Path.DirectorySeparatorChar + "*"))
        {
            var directory = Path.GetDirectoryName(path);
            if (Directory.Exists(directory))
            {
                _logger.LogDebug("Deleting {Directory} directory", directory);
                count += Directory.EnumerateFiles(directory, "*,*", SearchOption.AllDirectories).Count();
                Directory.Delete(directory, true);
                _logger.LogTrace("Finished deleting {Directory} directory with {FileCount} files", directory, count);
                return Task.FromResult(count);
            }

            return Task.FromResult(0);
        }

        if (Directory.Exists(path))
        {
            _logger.LogDebug("Deleting {Directory} directory", path);
            count += Directory.EnumerateFiles(path, "*,*", SearchOption.AllDirectories).Count();
            Directory.Delete(path, true);
            _logger.LogTrace("Finished deleting {Directory} directory with {FileCount} files", path, count);
            return Task.FromResult(count);
        }

        _logger.LogDebug("Deleting files matching {SearchPattern}", searchPattern);
        foreach (string file in Directory.EnumerateFiles(Folder, searchPattern, SearchOption.AllDirectories))
        {
            _logger.LogTrace("Deleting {Path}", file);
            File.Delete(file);
            count++;
        }

        _logger.LogTrace("Finished deleting {FileCount} files matching {SearchPattern}", count, searchPattern);
        return Task.FromResult(count);
    }

#pragma warning disable CA1816 // Dispose 方法应调用 SuppressFinalize
    public void Dispose()
#pragma warning restore CA1816 // Dispose 方法应调用 SuppressFinalize
    {
        //todo:
    }

    /// <summary>
    /// 判断文件是否存在
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Task<bool> ExistsAsync(string path)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);

        string normalizedPath = path.NormalizePath();
        _logger.LogTrace("Checking if {Path} exists", normalizedPath);
        return Task.FromResult(File.Exists(Path.Combine(Folder, normalizedPath)));
    }

    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<FileSpec?> GetFileInfoAsync(string path)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);

        string normalizedPath = path.NormalizePath();
        _logger.LogTrace("Getting file stream for {Path}", normalizedPath);


        var info = new FileInfo(Path.Combine(Folder, normalizedPath));
        if (!info.Exists)
        {
            _logger.LogError("Unable to get file info for {Path}: File Not Found", normalizedPath);
            return null!;
        }

        return await Task.FromResult(new FileSpec
        {
            Path = normalizedPath.Replace(Folder, String.Empty),
            Created = info.CreationTimeUtc,
            Modified = info.LastWriteTimeUtc,
            Size = info.Length
        });
    }


    /// <summary>
    /// 获取文件流,默认为读取模式
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        return GetFileStreamAsync(path, StreamMode.Read, cancellationToken);
    }

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="path"></param>
    /// <param name="streamMode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Task<Stream> GetFileStreamAsync(string path, StreamMode streamMode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);

        string normalizedPath = path.NormalizePath();
        string fullPath = Path.Combine(Folder, normalizedPath);
        EnsureDirectory(fullPath);

        var stream = streamMode == StreamMode.Read ? File.OpenRead(fullPath) : File.OpenWrite(fullPath);
        return Task.FromResult<Stream>(stream);
    }

    /// <summary>
    /// 获取分页文件列表
    /// </summary>
    /// <param name="pageSize"></param>
    /// <param name="searchPattern"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PagedFileListResult> GetPagedFileListAsync(int pageSize = 100, string? searchPattern = null, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0)
            return PagedFileListResult.Empty;

        if (string.IsNullOrEmpty(searchPattern))
            searchPattern = "*";

        searchPattern = searchPattern.NormalizePath();

        if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(Folder, searchPattern))))
        {
            _logger.LogTrace("Returning empty file list matching {SearchPattern}: Directory Not Found", searchPattern);
            return PagedFileListResult.Empty;
        }

        var result = new PagedFileListResult(s => Task.FromResult(GetFiles(searchPattern, 1, pageSize)));
        await result.NextPageAsync();
        return result;
    }

    /// <summary>
    /// 重命名文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="newPath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNullOrEmpty(newPath);

        string normalizedPath = path.NormalizePath();
        string normalizedNewPath = newPath.NormalizePath();
        _logger.LogDebug("Renaming {Path} to {NewPath}", normalizedPath, normalizedNewPath);

        try
        {
            lock (_lock)
            {
                var directory = Path.GetDirectoryName(normalizedNewPath);
                if (directory != null)
                {
                    _logger.LogInformation("Creating {Directory} directory", directory);
                    Directory.CreateDirectory(Path.Combine(Folder, directory));
                }

                string oldFullPath = Path.Combine(Folder, normalizedPath);
                string newFullPath = Path.Combine(Folder, normalizedNewPath);
                try
                {
                    File.Move(oldFullPath, newFullPath);
                }
                catch (IOException ex)
                {
                    _logger.LogDebug(ex, "Error renaming {Path} to {NewPath}: Deleting {NewFullPath}", normalizedPath, normalizedNewPath, newFullPath);
                    File.Delete(newFullPath);

                    _logger.LogTrace("Renaming {Path} to {NewPath}", normalizedPath, normalizedNewPath);
                    File.Move(oldFullPath, newFullPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renaming {Path} to {NewPath}", normalizedPath, normalizedNewPath);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public async Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(stream);

        try
        {
            using var fileStream = await GetFileStreamAsync(path, StreamMode.Write, cancellationToken);
            await stream.CopyToAsync(fileStream, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving {Path}: {Message}", path, ex.Message);
            return false;
        }
    }
}