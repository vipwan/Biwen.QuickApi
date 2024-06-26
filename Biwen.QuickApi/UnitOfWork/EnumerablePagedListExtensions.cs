using Biwen.QuickApi.UnitOfWork.Pagenation;

namespace Biwen.QuickApi.UnitOfWork;

/// <summary>
/// Enumerable分页扩展
/// </summary>
public static class EnumerablePagedListExtensions
{

    public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageIndex, int pageSize, int indexFrom = 0)
        => PagedList.Create(source, pageIndex, pageSize, indexFrom);


    public static IPagedList<TResult> ToPagedList<TSource, TResult>(
        this IPagedList<TSource> source,
        Func<IEnumerable<TSource>, IEnumerable<TResult>> converter)
        => PagedList.Create<TSource, TResult>(source, converter);
}