namespace Biwen.QuickApi.UnitOfWork.Pagenation
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IPagedList{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the data to page</typeparam>
    public class PagedList<T> : IPagedList<T>
    {
        /// <inheritdoc/>
        public int PageIndex { get; private set; }


        /// <inheritdoc/>
        public int PageSize { get; private set; }

        /// <inheritdoc/>
        public int TotalCount { get; private set; }

        /// <inheritdoc/>
        public int TotalPages { get; private set; }


        /// <inheritdoc/>
        public int IndexFrom { get; private set; }

        /// <inheritdoc/>
        public IList<T> Items { get; private set; }

        /// <inheritdoc/>
        public bool HasPreviousPage => PageIndex - IndexFrom > 0;

        /// <inheritdoc/>
        public bool HasNextPage => PageIndex - IndexFrom + 1 < TotalPages;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="pageIndex">The index of the page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="indexFrom">The index from.</param>
        /// <param name="totalCount">Total items in collection. Default is null. Will check source to count</param>
        internal PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int indexFrom, int? totalCount = null)
        {
            if (indexFrom > pageIndex)
            {
                throw new ArgumentException(
                    $"indexFrom: {indexFrom} > pageIndex: {pageIndex}, must indexFrom <= pageIndex");
            }

            if (source is IQueryable<T> queryable)
            {
                PageIndex = pageIndex;
                PageSize = pageSize;
                IndexFrom = indexFrom;
                TotalCount = totalCount ?? queryable.Count();
                TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
                Items = queryable.Skip((PageIndex - IndexFrom) * PageSize).Take(PageSize).ToList();
            }
            else
            {
                var enumerable = source.ToList();
                PageIndex = pageIndex;
                PageSize = pageSize;
                IndexFrom = indexFrom;
                TotalCount = totalCount ?? enumerable.Count;
                TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
                Items = enumerable
                    .Skip((PageIndex - IndexFrom) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}" /> class.
        /// </summary>
        internal PagedList() => Items = Array.Empty<T>();

        /// <summary>
        /// Creates an instance with predefined parameters.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="indexFrom"></param>
        /// <param name="count"></param>
        public PagedList(
            IEnumerable<T> source,
            int pageIndex,
            int pageSize,
            int indexFrom,
            int count)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            IndexFrom = indexFrom;
            TotalCount = count;
            Items = source.ToList();
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
    }
}
