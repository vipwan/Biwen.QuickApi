// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:55:19 EnumerablePagedListExtensions.cs

using Biwen.QuickApi.UnitOfWork.Pagenation;

namespace Biwen.QuickApi;

/// <summary>
/// Enumerable分页扩展
/// </summary>
public static class EnumerablePagedListExtensions
{
    /// <summary>
    /// Creates a paginated list from a collection of items.
    /// </summary>
    /// <typeparam name="T">Represents the type of elements in the collection being paginated.</typeparam>
    /// <param name="source">The collection of items to be paginated.</param>
    /// <param name="pageIndex">Specifies the index of the page to retrieve.</param>
    /// <param name="pageSize">Determines the number of items to include on each page.</param>
    /// <param name="indexFrom">Indicates the starting index for pagination, defaulting to zero.</param>
    /// <returns>Returns a paginated list containing the specified page of items.</returns>
    public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageIndex, int pageSize, int indexFrom = 0)
        => PagedList.Create(source, pageIndex, pageSize, indexFrom);

    /// <summary>
    /// Converts a source paged list into a new paged list of a different type using a specified converter function.
    /// </summary>
    /// <typeparam name="TSource">Represents the type of elements in the original paged list.</typeparam>
    /// <typeparam name="TResult">Represents the type of elements in the resulting paged list after conversion.</typeparam>
    /// <param name="source">The original paged list that contains elements to be converted.</param>
    /// <param name="converter">A function that transforms a collection of the original elements into a collection of the new type.</param>
    /// <returns>A new paged list containing the converted elements.</returns>
    public static IPagedList<TResult> ToPagedList<TSource, TResult>(
        this IPagedList<TSource> source,
        Func<IEnumerable<TSource>, IEnumerable<TResult>> converter)
        => PagedList.Create<TSource, TResult>(source, converter);
}