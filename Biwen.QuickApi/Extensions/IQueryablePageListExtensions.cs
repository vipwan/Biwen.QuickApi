// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:55:24 IQueryablePageListExtensions.cs

using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi;

/// <summary>
/// IQueryable 分页扩展
/// </summary>
public static class IQueryablePageListExtensions
{
    /// <summary>
    /// 分页扩展
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="pageIndex">0:开始</param>
    /// <param name="pageSize">默认:20</param>
    /// <param name="indexFrom">默认:0,表示查询从0开始</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageIndex = 0,
        int pageSize = 20, int indexFrom = 0, CancellationToken cancellationToken = default)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentException($"pageIndex: {pageIndex}, must pageIndex >= 0");
        }

        if (indexFrom > pageIndex)
        {
            throw new ArgumentException(
                $"indexFrom: {indexFrom} > pageIndex: {pageIndex}, must indexFrom <= pageIndex");
        }

        var count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await source.Skip((pageIndex - indexFrom) * pageSize)
            .Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedList<T>(items, pageIndex, pageSize, indexFrom, count);
    }
}