using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.UnitOfWork;

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
    /// <param name="indexFrom"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageIndex = 0,
        int pageSize = 20, int indexFrom = 0, CancellationToken cancellationToken = default)
    {
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