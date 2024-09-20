// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:01:03 AliyunOssStorage.cs

using Aliyun.OSS;
using Biwen.QuickApi.Serializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.RegularExpressions;

namespace Biwen.QuickApi.Storage.AliyunOss;

public class AliyunOssStorage : IFileStorage
{
    public AliyunOssStorage(IServiceProvider serviceProvider, string connectionString)
    {
        _serializer = serviceProvider.GetService<ISerializer>() ?? DefaultSerializer.Instance;
        _logger = serviceProvider.GetRequiredService<ILogger<AliyunOssStorage>>();
        //var options = serviceProvider.GetRequiredService<IOptions<AliyunOssOptions>>();
        var connection = new AliyunFileStorageConnectionStringBuilder(connectionString);
        _client = new OssClient(connection.Endpoint, connection.AccessKey, connection.SecretKey);

        _bucket = connection.Bucket;
        if (DoesBucketExist(_bucket))
            return;

        //如果Bucket不存在，则创建Bucket
        _client.CreateBucket(_bucket);
        _logger.LogDebug("Created {Bucket}", _bucket);
    }

    private readonly string _bucket;
    private readonly ISerializer _serializer;
    private readonly ILogger<AliyunOssStorage> _logger;
    private readonly OssClient _client;

    public ISerializer Serializer => _serializer;


    #region helper

    private bool IsNotFoundException(Exception ex)
    {
        if (ex is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.InnerExceptions)
            {
                if (IsNotFoundException(innerException))
                    return true;
            }
        }

        if (ex is WebException { Response: HttpWebResponse response })
            return response.StatusCode == HttpStatusCode.NotFound;

        return false;
    }

    private bool DoesBucketExist(string bucketName)
    {
        _logger.LogTrace("Checking if bucket {Bucket} exists", _bucket);
        try
        {
            return _client.DoesBucketExist(bucketName);
        }
        catch (Exception ex) when (IsNotFoundException(ex))
        {
            _logger.LogError(ex, "Unable to check if {Bucket} bucket exists: {Message}", bucketName, ex.Message);
            return false;
        }
    }

    private string? NormalizePath(string path)
    {
        return path?.Replace('\\', '/');
    }

    private async Task<List<FileSpec>> GetFileListAsync(string? searchPattern = null, int? limit = null, int? skip = null, CancellationToken cancellationToken = default)
    {
        if (limit is <= 0) return [];

        var criteria = GetRequestCriteria(searchPattern);
        limit = limit.GetValueOrDefault(int.MaxValue);
        int totalLimit = limit < int.MaxValue
            ? skip.GetValueOrDefault() + limit.Value
            : int.MaxValue;

        string? marker = null;
        var blobs = new List<OssObjectSummary>();
        do
        {
            var listing = await Task.Factory.FromAsync(
                _client.BeginListObjects,
                _client.EndListObjects,
                new ListObjectsRequest(_bucket)
                {
                    Prefix = criteria.Prefix,
                    Marker = marker,
                    MaxKeys = limit
                },
                null
            );
            marker = listing.NextMarker;

            foreach (var blob in listing.ObjectSummaries)
            {
                if (criteria.Pattern != null && !criteria.Pattern.IsMatch(blob.Key))
                {
                    _logger.LogTrace("Skipping {Path}: Doesn't match pattern", blob.Key);
                    continue;
                }

                blobs.Add(blob);
            }
        } while (!cancellationToken.IsCancellationRequested && !String.IsNullOrEmpty(marker) && blobs.Count < totalLimit);

        if (skip.HasValue)
            blobs = blobs.Skip(skip.Value).ToList();

        if (limit.HasValue)
            blobs = blobs.Take(limit.Value).ToList();

        return blobs.Select(blob => new FileSpec
        {
            Path = blob.Key,
            Size = blob.Size,
            Created = blob.LastModified,
            Modified = blob.LastModified
        }).ToList();
    }

    private async Task<NextPageResult> GetFilesAsync(string? searchPattern, int page, int pageSize, CancellationToken cancellationToken)
    {
        var criteria = GetRequestCriteria(searchPattern);

        int pagingLimit = pageSize;
        int skip = (page - 1) * pagingLimit;
        if (pagingLimit < int.MaxValue)
            pagingLimit++;

        string? marker = null;
        int totalLimit = pagingLimit < int.MaxValue ? skip + pagingLimit : int.MaxValue;
        var blobs = new List<OssObjectSummary>();
        do
        {
            var listing = await Task.Factory.FromAsync(
                _client.BeginListObjects,
                _client.EndListObjects,
                new ListObjectsRequest(_bucket)
                {
                    Prefix = criteria.Prefix,
                    Marker = marker,
                    MaxKeys = pagingLimit
                },
                null
            );
            marker = listing.NextMarker;

            foreach (var blob in listing.ObjectSummaries)
            {
                if (criteria.Pattern != null && !criteria.Pattern.IsMatch(blob.Key))
                {
                    _logger.LogTrace("Skipping {Path}: Doesn't match pattern", blob.Key);
                    continue;
                }

                blobs.Add(blob);
            }
        } while (!cancellationToken.IsCancellationRequested && !String.IsNullOrEmpty(marker) && blobs.Count < totalLimit);

        var list = blobs
            .Skip(skip)
            .Take(pagingLimit)
            .Select(blob => new FileSpec
            {
                Path = blob.Key,
                Size = blob.Size,
                Created = blob.LastModified,
                Modified = blob.LastModified
            })
            .ToList();

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
            NextPageFunc = hasMore ? _ => GetFilesAsync(searchPattern, page + 1, pageSize, cancellationToken) : null
        };
    }


    private class SearchCriteria
    {
        public string? Prefix { get; set; }
        public Regex? Pattern { get; set; }
    }

    private SearchCriteria GetRequestCriteria(string? searchPattern)
    {
        if (string.IsNullOrEmpty(searchPattern))
            return new SearchCriteria { Prefix = string.Empty };
        string normalizedSearchPattern = NormalizePath(searchPattern) ?? string.Empty;
        //var wildcardPos = normalizedSearchPattern.IndexOf('*');
        //bool hasWildcard = wildcardPos >= 0;
        var hasWildcard = normalizedSearchPattern.Contains('*');
        var prefix = normalizedSearchPattern;
        Regex? patternRegex = null;

        if (hasWildcard)
        {
            patternRegex = new Regex($"^{Regex.Escape(normalizedSearchPattern).Replace("\\*", ".*?")}$");
            int slashPos = normalizedSearchPattern.LastIndexOf('/');
            prefix = slashPos >= 0 ? normalizedSearchPattern.Substring(0, slashPos) : string.Empty;
        }

        return new SearchCriteria
        {
            Prefix = prefix,
            Pattern = patternRegex
        };
    }

    private bool IsSuccessful(HttpStatusCode code)
    {
        return (int)code < 400;
    }

    #endregion

    /// <inheritdoc/>
    public async Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNullOrEmpty(targetPath);

        var normalizedPath = NormalizePath(path);
        var normalizedTargetPath = NormalizePath(targetPath);
        _logger.LogDebug("Copying {Path} to {TargetPath}", normalizedPath, normalizedTargetPath);

        try
        {
            var copyResult = await Task.Factory.FromAsync(
                _client.BeginCopyObject,
                _client.EndCopyResult,
                new CopyObjectRequest(_bucket, normalizedPath, _bucket, normalizedTargetPath),
                null
            );
            return copyResult.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying {Path} to {TargetPath}: {Message}", normalizedPath, normalizedTargetPath, ex.Message);
            return false;
        }
    }
    /// <inheritdoc/>
    public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);

        var normalizedPath = NormalizePath(path);
        _logger.LogTrace("Deleting {Path}", normalizedPath);

        try
        {
            var deleteResult = _client.DeleteObject(_bucket, normalizedPath);
            return Task.FromResult(deleteResult.HttpStatusCode == HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete {Path}: File not found", normalizedPath);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc/>
    public async Task<int> DeleteFilesAsync(string? searchPattern = null, CancellationToken cancellation = default)
    {
        var files = await GetFileListAsync(searchPattern, cancellationToken: cancellation);
        _logger.LogInformation("Deleting {FileCount} files matching {SearchPattern}", files.Count, searchPattern);
        var result = _client.DeleteObjects(new DeleteObjectsRequest(_bucket, files.Select(spec => spec.Path).ToList()));
        if (result.HttpStatusCode != HttpStatusCode.OK)
            throw new Exception($"[{result.HttpStatusCode}] Unable to delete files");

        int count = result.Keys?.Length ?? 0;
        _logger.LogTrace("Finished deleting {FileCount} files matching {SearchPattern}", count, searchPattern);
        return count;
    }

#pragma warning disable CA1816 // Dispose 方法应调用 SuppressFinalize
    public void Dispose()
#pragma warning restore CA1816 // Dispose 方法应调用 SuppressFinalize
    {
        //todo:
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string path)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);

        var normalizedPath = NormalizePath(path);
        _logger.LogTrace("Checking if {Path} exists", normalizedPath);

        try
        {
            return Task.FromResult(_client.DoesObjectExist(_bucket, normalizedPath));
        }
        catch (Exception ex) when (IsNotFoundException(ex))
        {
            _logger.LogDebug(ex, "Unable to check if {Path} exists: {Message}", normalizedPath, ex.Message);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc/>
    public async Task<FileSpec?> GetFileInfoAsync(string path)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);
        var normalizedPath = NormalizePath(path);
        _logger.LogTrace("Getting file info for {Path}", normalizedPath);

        try
        {
            var metadata = _client.GetObjectMetadata(_bucket, normalizedPath);
            return await Task.FromResult(new FileSpec
            {
                Path = normalizedPath!,
                Size = metadata.ContentLength,
                Created = metadata.LastModified,
                Modified = metadata.LastModified
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get file info for {Path}: {Message}", path, ex.Message);
            return null!;
        }
    }

    /// <inheritdoc/>
    public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        return GetFileStreamAsync(path, StreamMode.Read, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Stream> GetFileStreamAsync(string path, StreamMode streamMode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);

        if (streamMode is StreamMode.Write)
            throw new NotSupportedException($"Stream mode {streamMode} is not supported.");

        var normalizedPath = NormalizePath(path);
        _logger.LogTrace("Getting file stream for {Path}", normalizedPath);

        using var response = await Task.Factory.FromAsync(_client.BeginGetObject, _client.EndGetObject, new GetObjectRequest(_bucket, normalizedPath), null);
        if (!IsSuccessful(response.HttpStatusCode))
        {
            _logger.LogError("[{HttpStatusCode}] Unable to get file stream for {Path}", response.HttpStatusCode, normalizedPath);
            return null!;
        }

        return response.ResponseStream;
    }

    public async Task<PagedFileListResult> GetPagedFileListAsync(int pageSize = 100, string? searchPattern = null, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0)
            return PagedFileListResult.Empty;

        var result = new PagedFileListResult(_ => GetFilesAsync(searchPattern, 1, pageSize, cancellationToken));
        await result.NextPageAsync();
        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNullOrEmpty(newPath);

        var normalizedPath = NormalizePath(path);
        var normalizedNewPath = NormalizePath(newPath);

        _logger.LogDebug("Renaming {Path} to {NewPath}", normalizedPath, normalizedNewPath);

        //复制文件后删除原文件
        return await CopyFileAsync(normalizedPath!, normalizedNewPath!, cancellationToken) &&
               await DeleteFileAsync(normalizedPath!, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(stream);

        var normalizedPath = NormalizePath(path);
        _logger.LogTrace("Saving {Path}", normalizedPath);

        var seekableStream = stream.CanSeek ? stream : new MemoryStream();
        if (!stream.CanSeek)
        {
            await stream.CopyToAsync(seekableStream, cancellationToken);
            seekableStream.Seek(0, SeekOrigin.Begin);
        }

        try
        {
            var putResult = await Task.Factory.FromAsync(_client.BeginPutObject, _client.EndPutObject, new PutObjectRequest(_bucket, normalizedPath, seekableStream), null);
            return putResult.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving {Path}: {Message}", normalizedPath, ex.Message);
            return false;
        }
        finally
        {
            if (!stream.CanSeek)
                seekableStream.Dispose();
        }
    }
}