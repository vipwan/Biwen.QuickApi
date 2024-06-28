namespace Biwen.QuickApi.UnitOfWork.Pagenation;

/// <summary>
/// Provides some help methods for <see cref="IPagedList{T}"/> interface.
/// </summary>
public static class PagedList
{
    /// <summary>
    /// Creates an empty of <see cref="IPagedList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type for paging </typeparam>
    /// <returns>An empty instance of <see cref="IPagedList{T}"/>.</returns>
    public static IPagedList<T> Empty<T>() => new PagedList<T>();

    /// <summary>
    /// Creates a new instance of <see cref="IPagedList{TResult}"/> from source of <see cref="IPagedList{TSource}"/> instance.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="converter">The converter.</param>
    /// <returns>An instance of <see cref="IPagedList{TResult}"/>.</returns>
    public static IPagedList<TResult> From<TSource, TResult>(
        IPagedList<TSource> source,
        Func<IEnumerable<TSource>, IEnumerable<TResult>> converter)
        => new PagedList<TSource, TResult>(source, converter);

    /// <summary>
    /// Creates a new instance of <see cref="IPagedList{TResult}"/> from parameters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="indexFrom"></param>
    /// <returns></returns>
    public static IPagedList<T> Create<T>(
        IEnumerable<T> items,
        int pageIndex,
        int pageSize,
        int indexFrom)
        => new PagedList<T>(items, pageIndex, pageSize, indexFrom);

    /// <summary>
    /// Creates a new instance of &lt;see cref="IPagedList{TResult}"/&gt; from source of &lt;see cref="IPagedList{TSource}"/&gt; instance.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="converter">The converter.</param>
    /// <returns>An instance of <see cref="IPagedList{TResult}"/>.</returns>
    public static IPagedList<TResult> Create<TSource, TResult>(
            IPagedList<TSource> source,
            Func<IEnumerable<TSource>, IEnumerable<TResult>> converter)
        => new PagedList<TSource, TResult>(source, converter);
}

