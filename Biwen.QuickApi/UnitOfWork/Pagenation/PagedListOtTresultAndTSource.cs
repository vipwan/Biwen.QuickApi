// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:55:00 PagedListOtTresultAndTSource.cs

namespace Biwen.QuickApi.UnitOfWork.Pagenation;

/// <summary>
/// Provides the implementation of the <see cref="IPagedList{T}"/> and converter.
/// </summary>
/// <typeparam name="TSource">The type of the source.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
internal class PagedList<TSource, TResult> : IPagedList<TResult>
{

    /// <inheritdoc/>
    public int PageIndex { get; }

    /// <inheritdoc/>
    public int PageSize { get; }

    /// <inheritdoc/>
    public int TotalCount { get; }

    /// <inheritdoc/>
    public int TotalPages { get; }

    /// <inheritdoc/>
    public int IndexFrom { get; }

    /// <inheritdoc/>
    public IList<TResult> Items { get; }

    /// <inheritdoc/>
    public bool HasPreviousPage => PageIndex - IndexFrom > 0;

    /// <inheritdoc/>
    public bool HasNextPage => PageIndex - IndexFrom + 1 < TotalPages;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedList{TSource, TResult}" /> class.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="converter">The converter.</param>
    /// <param name="pageIndex">The index of the page.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="indexFrom">The index from.</param>
    public PagedList
    (
        IEnumerable<TSource> source,
        Func<IEnumerable<TSource>, IEnumerable<TResult>> converter,
        int pageIndex,
        int pageSize,
        int indexFrom)
    {
        if (indexFrom > pageIndex)
        {
            throw new ArgumentException(
                $"indexFrom: {indexFrom} > pageIndex: {pageIndex}, must indexFrom <= pageIndex");
        }

        if (source is IQueryable<TSource> querable)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            IndexFrom = indexFrom;
            TotalCount = querable.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            var items = querable.Skip((PageIndex - IndexFrom) * PageSize).Take(PageSize).ToArray();

            Items = new List<TResult>(converter(items));
        }
        else
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            IndexFrom = indexFrom;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            var items = source.Skip((PageIndex - IndexFrom) * PageSize).Take(PageSize).ToArray();

            Items = new List<TResult>(converter(items));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedList{TSource, TResult}" /> class.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="converter">The converter.</param>
    public PagedList(IPagedList<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> converter)
    {
        PageIndex = source.PageIndex;
        PageSize = source.PageSize;
        IndexFrom = source.IndexFrom;
        TotalCount = source.TotalCount;
        TotalPages = source.TotalPages;

        Items = new List<TResult>(converter(source.Items));
    }
}
