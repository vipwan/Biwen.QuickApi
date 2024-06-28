using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Biwen.QuickApi.UnitOfWork;

/// <summary>
/// Represents a default generic repository implements the <see cref="IRepository{TEntity}"/> interface.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public Repository(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.Set<TEntity>();
    }

    /// <inheritdoc/>
    public void ChangeEntityState(TEntity entity, EntityState state) => _dbContext.Entry(entity).State = state;


    /// <inheritdoc/>
    public void ChangeTable(string table)
    {
        if (_dbContext.Model.FindEntityType(typeof(TEntity)) is IConventionEntityType relational)
        {
            relational.SetTableName(table);
        }
    }

    /// <inheritdoc/>
    public IQueryable<TEntity> GetAll(bool disableTracking = true) =>
        disableTracking
            ? _dbSet.AsNoTracking()
            : _dbSet;


    /// <inheritdoc/>
    public IQueryable<TResult> GetAll<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        bool disableTracking = true) =>
        disableTracking
            ? _dbSet.AsNoTracking().Select(selector)
            : _dbSet.Select(selector);

    public IQueryable<TResult> GetAll<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        bool disableTracking = true)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return query.Select(selector);
    }


    /// <inheritdoc/>
    public IQueryable<TEntity> GetAll(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy is not null
            ? orderBy(query)
            : query;
    }


    /// <inheritdoc/>
    public IQueryable<TResult> GetAll<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy != null
            ? orderBy(query).Select(selector)
            : query.Select(selector);
    }


    /// <inheritdoc/>
    public async Task<IList<TEntity>> GetAllAsync(bool disableTracking = true)
        => disableTracking
            ? await _dbSet.AsNoTracking().ToListAsync()
            : await _dbSet.ToListAsync();


    /// <inheritdoc/>
    public async Task<IList<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, TResult>> selector, bool disableTracking = true) =>
        disableTracking
            ? await _dbSet.AsNoTracking().Select(selector).ToListAsync()
            : await _dbSet.Select(selector).ToListAsync();

    /// <inheritdoc/>
    public async Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        if (orderBy is not null)
        {
            return await orderBy(query).ToListAsync();
        }

        return await query.ToListAsync();
    }


    /// <inheritdoc/>
    public async Task<IList<TResult>> GetAllAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy is not null
            ? await orderBy(query).Select(selector).ToListAsync()
            : await query.Select(selector).ToListAsync();
    }


    /// <inheritdoc/>
    public IPagedList<TEntity> GetPagedList(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int pageIndex = 0,
        int pageSize = 20,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy is not null
            ? orderBy(query).ToPagedList(pageIndex, pageSize)
            : query.ToPagedList(pageIndex, pageSize);
    }

    /// <inheritdoc/>
    public Task<IPagedList<TEntity>> GetPagedListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int pageIndex = 0,
        int pageSize = 20,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy is not null
            ? orderBy(query).ToPagedListAsync(pageIndex, pageSize, 0, cancellationToken)
            : query.ToPagedListAsync(pageIndex, pageSize, 0, cancellationToken);
    }

    /// <inheritdoc/>
    public IPagedList<TResult> GetPagedList<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int pageIndex = 0,
        int pageSize = 20,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
        where TResult : class
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy is not null
            ? orderBy(query).Select(selector).ToPagedList(pageIndex, pageSize)
            : query.Select(selector).ToPagedList(pageIndex, pageSize);
    }

    /// <inheritdoc/>
    public Task<IPagedList<TResult>> GetPagedListAsync<TResult>(Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int pageIndex = 0,
        int pageSize = 20,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false,
        CancellationToken cancellationToken = default
        )
        where TResult : class
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include != null)
        {
            query = include(query);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy != null
            ? orderBy(query).Select(selector).ToPagedListAsync(pageIndex, pageSize, 0, cancellationToken)
            : query.Select(selector).ToPagedListAsync(pageIndex, pageSize, 0, cancellationToken);
    }


    /// <inheritdoc/>
    public TEntity? GetFirstOrDefault(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy is not null
            ? orderBy(query).FirstOrDefault()
            : query.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy is not null
            ? await orderBy(query).FirstOrDefaultAsync()
            : await query.FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public TResult? GetFirstOrDefault<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy is not null
            ? orderBy(query).Select(selector).FirstOrDefault()
            : query.Select(selector).FirstOrDefault();
    }

    /// <inheritdoc/>
    public IQueryable<TEntity> FromSql(string sql, params object[] parameters) => _dbSet.FromSqlRaw(sql, parameters);

    /// <inheritdoc/>
    public TEntity? Find(params object[] keyValues) => _dbSet.Find(keyValues);

    /// <inheritdoc/>
    public ValueTask<TEntity?> FindAsync(params object[] keyValues) => _dbSet.FindAsync(keyValues);

    /// <inheritdoc/>
    public ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken cancellationToken) => _dbSet.FindAsync(keyValues, cancellationToken);

    /// <inheritdoc/>
    public int Count(Expression<Func<TEntity, bool>>? predicate = null) =>
        predicate is null
            ? _dbSet.Count()
            : _dbSet.Count(predicate);

    /// <inheritdoc/>
    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        predicate is null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);

    /// <inheritdoc/>
    public long LongCount(Expression<Func<TEntity, bool>>? predicate = null) =>
        predicate is null
            ? _dbSet.LongCount()
            : _dbSet.LongCount(predicate);

    /// <inheritdoc/>
    public async Task<long> LongCountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        predicate is null
            ? await _dbSet.LongCountAsync(cancellationToken)
            : await _dbSet.LongCountAsync(predicate, cancellationToken);

    /// <inheritdoc/>
    public bool Exists(Expression<Func<TEntity, bool>>? predicate = null) =>
        predicate is null
            ? _dbSet.Any()
            : _dbSet.Any(predicate);

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>>? selector = null,
        CancellationToken cancellationToken = default) =>
        selector is null
            ? await _dbSet.AnyAsync(cancellationToken)
            : await _dbSet.AnyAsync(selector, cancellationToken);

    /// <inheritdoc/>
    public T? Max<T>(
        Expression<Func<TEntity, T>> selector,
        Expression<Func<TEntity, bool>>? predicate = null) =>
        predicate is null
            ? _dbSet.Max(selector)
            : _dbSet.Where(predicate).Max(selector);

    /// <inheritdoc/>
    public Task<T> MaxAsync<T>(
        Expression<Func<TEntity, T>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        predicate is null
            ? _dbSet.MaxAsync(selector, cancellationToken)
            : _dbSet.Where(predicate).MaxAsync(selector, cancellationToken);

    /// <inheritdoc/>
    public T? Min<T>(
        Expression<Func<TEntity, T>> selector,
        Expression<Func<TEntity, bool>>? predicate = null) =>
        predicate is null
            ? _dbSet.Min(selector)
            : _dbSet.Where(predicate).Min(selector);

    /// <inheritdoc/>
    public Task<T> MinAsync<T>(
        Expression<Func<TEntity, T>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        predicate is null
            ? _dbSet.MinAsync(selector, cancellationToken)
            : _dbSet.Where(predicate).MinAsync(selector, cancellationToken);

    /// <inheritdoc/>
    public decimal Average(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null) =>
        predicate is null
            ? _dbSet.Average(selector)
            : _dbSet.Where(predicate).Average(selector);

    /// <inheritdoc/>
    public Task<decimal> AverageAsync(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        predicate is null
            ? _dbSet.AverageAsync(selector, cancellationToken)
            : _dbSet.Where(predicate).AverageAsync(selector, cancellationToken);

    /// <inheritdoc/>
    public decimal Sum(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null) =>
        predicate is null
            ? _dbSet.Sum(selector)
            : _dbSet.Where(predicate).Sum(selector);

    /// <inheritdoc/>
    public Task<decimal> SumAsync(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        predicate is null
            ? _dbSet.SumAsync(selector, cancellationToken)
            : _dbSet.Where(predicate).SumAsync(selector, cancellationToken);

    #region Insert

    /// <inheritdoc/>
    public TEntity Insert(TEntity entity) => _dbSet.Add(entity).Entity;

    /// <inheritdoc/>
    public void Insert(params TEntity[] entities) => _dbSet.AddRange(entities);

    /// <inheritdoc/>
    public void Insert(IEnumerable<TEntity> entities) => _dbSet.AddRange(entities);

    /// <inheritdoc/>
    public ValueTask<EntityEntry<TEntity>> InsertAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        _dbSet.AddAsync(entity, cancellationToken);

    /// <inheritdoc/>
    public Task InsertAsync(params TEntity[] entities) => _dbSet.AddRangeAsync(entities);

    /// <inheritdoc/>
    public Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) => _dbSet.AddRangeAsync(entities, cancellationToken);

    #endregion

    #region Update

    /// <inheritdoc/>
    public void Update(TEntity entity) => _dbSet.Update(entity);

    /// <inheritdoc/>
    public void Update(params TEntity[] entities) => _dbSet.UpdateRange(entities);

    /// <inheritdoc/>
    public void Update(IEnumerable<TEntity> entities) => _dbSet.UpdateRange(entities);

    /// <inheritdoc/>
    public int ExecuteUpdate(Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> predicate)
        => _dbSet.ExecuteUpdate(predicate);

    /// <inheritdoc/>
    public Task<int> ExecuteUpdateAsync(
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> predicate,
        CancellationToken cancellationToken)
        => _dbSet.ExecuteUpdateAsync(predicate, cancellationToken);

    #endregion

    #region Delete


    /// <inheritdoc/>
    public void Delete(IEnumerable<TEntity> entities) => _dbSet.RemoveRange(entities);

    /// <inheritdoc/>
    public void Delete(params TEntity[] entities) => _dbSet.RemoveRange(entities);
    /// <inheritdoc/>
    /// 
    public void Delete(TEntity entity) => _dbSet.Remove(entity);

    /// <inheritdoc/>
    public void Delete(object id)
    {
        var entity = _dbSet.Find(id);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    /// <inheritdoc/>
    public int ExecuteDelete() => _dbSet.ExecuteDelete();

    /// <inheritdoc/>
    public Task<int> ExecuteDeleteAsync(CancellationToken cancellationToken = default) => _dbSet.ExecuteDeleteAsync(cancellationToken);

    #endregion
}