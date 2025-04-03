// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi 作者: 万雅虎 Github: https://github.com/vipwan
// 修改日期: 2025-04-03 UnitOfWork.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Biwen.QuickApi.UnitOfWork;

/// <summary>
/// 工作单元模式的默认实现，实现了 <see cref="T:IUnitOfWork"/> 和 <see cref="T:IUnitOfWork{TContext}"/> 接口
/// </summary>
/// <typeparam name="TContext">数据库上下文类型</typeparam>
public sealed class UnitOfWork<TContext> : IRepositoryFactory, IUnitOfWork<TContext>
    where TContext : DbContext
{
    #region 字段

    private bool _disposed;
    private Dictionary<Type, object>? _repositories;

    #endregion

    /// <summary>
    /// 初始化 <see cref="UnitOfWork{TContext}"/> 类的新实例
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public UnitOfWork(TContext context)
    {
        DbContext = context ?? throw new ArgumentNullException(nameof(context));
        LastSaveChangesResult = new SaveChangesResult();
    }

    #region 属性

    /// <summary>
    /// 获取数据库上下文
    /// </summary>
    /// <returns><typeparamref name="TContext"/> 类型的实例</returns>
    public TContext DbContext { get; }

    /// <summary>
    /// 获取最后一次保存操作的结果
    /// </summary>
    public SaveChangesResult LastSaveChangesResult { get; }

    #endregion

    #region 方法

    /// <summary>
    /// 异步开始一个事务
    /// </summary>
    /// <param name="useIfExists">如果已存在事务，是否使用现有事务</param>
    /// <returns>数据库事务对象</returns>
    public Task<IDbContextTransaction> BeginTransactionAsync(bool useIfExists = false)
    {
        var transaction = DbContext.Database.CurrentTransaction;
        if (transaction == null)
        {
            return DbContext.Database.BeginTransactionAsync();
        }

        return useIfExists ? Task.FromResult(transaction) : DbContext.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// 同步开始一个事务
    /// </summary>
    /// <param name="useIfExists">如果已存在事务，是否使用现有事务</param>
    /// <returns>数据库事务对象</returns>
    public IDbContextTransaction BeginTransaction(bool useIfExists = false)
    {
        var transaction = DbContext.Database.CurrentTransaction;
        if (transaction == null)
        {
            return DbContext.Database.BeginTransaction();
        }

        return useIfExists ? transaction : DbContext.Database.BeginTransaction();
    }

    /// <summary>
    /// 启用或禁用数据库上下文的自动变更检测
    /// </summary>
    /// <param name="value">是否启用自动变更检测</param>
    public void SetAutoDetectChanges(bool value) => DbContext.ChangeTracker.AutoDetectChangesEnabled = value;

    /// <summary>
    /// 获取指定实体类型的仓储
    /// </summary>
    /// <param name="hasCustomRepository">是否使用自定义仓储</param>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <returns>实现 <see cref="IRepository{TEntity}"/> 接口的实例</returns>
    public IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : class
    {
        _repositories ??= new Dictionary<Type, object>();

        // 处理自定义仓储
        if (hasCustomRepository)
        {
            var customRepo = DbContext.GetService<IRepository<TEntity>>();
            if (customRepo != null)
            {
                return customRepo;
            }
        }

        var type = typeof(TEntity);
        if (!_repositories.TryGetValue(type, out object? value))
        {
            value = new Repository<TEntity>(DbContext);
            _repositories[type] = value;
        }

        return (IRepository<TEntity>)value;
    }

    /// <summary>
    /// 执行指定的SQL命令
    /// </summary>
    /// <param name="sql">原始SQL语句</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>受影响的行数</returns>
    public int ExecuteSqlCommand(string sql, params object[] parameters) => DbContext.Database.ExecuteSqlRaw(sql, parameters);

    /// <summary>
    /// 异步执行指定的SQL命令
    /// </summary>
    /// <param name="sql">原始SQL语句</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>受影响的行数</returns>
    public Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters) => DbContext.Database.ExecuteSqlRawAsync(sql, parameters);

    /// <summary>
    /// 使用原始SQL查询获取指定实体类型的数据
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="sql">原始SQL语句</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>包含满足SQL条件的元素的 <see cref="IQueryable{T}"/> 对象</returns>
    public IQueryable<TEntity> FromSqlRaw<TEntity>(string sql, params object[] parameters) where TEntity : class
        => DbContext.Set<TEntity>().FromSqlRaw(sql, parameters);

    /// <summary>
    /// 将所有在此上下文中的更改保存到数据库
    /// </summary>
    /// <returns>写入数据库的状态条目数</returns>
    public int SaveChanges()
    {
        try
        {
            return DbContext.SaveChanges();
        }
        catch (Exception exception)
        {
            LastSaveChangesResult.Exception = exception;
            return 0;
        }
    }

    /// <summary>
    /// 异步将所有在此工作单元中的更改保存到数据库
    /// </summary>
    /// <returns>表示异步保存操作的任务，任务结果包含写入数据库的状态条目数</returns>
    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await DbContext.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            LastSaveChangesResult.Exception = exception;
            return 0;
        }
    }

    /// <summary>
    /// 使用分布式事务将所有在此上下文中的更改保存到数据库
    /// </summary>
    /// <param name="unitOfWorks">可选的 <see cref="T:IUnitOfWork"/> 数组</param>
    /// <returns>表示异步保存操作的任务，任务结果包含写入数据库的状态条目数</returns>
    public async Task<int> SaveChangesAsync(params IUnitOfWork[] unitOfWorks)
    {
        var count = 0;
        foreach (var unitOfWork in unitOfWorks)
        {
            count += await unitOfWork.SaveChangesAsync();
        }

        count += await SaveChangesAsync();
        return count;
    }

    /// <summary>
    /// 执行与释放或重置非托管资源相关的应用程序定义的任务
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 执行与释放或重置非托管资源相关的应用程序定义的任务
    /// </summary>
    /// <param name="disposing">是否正在释放资源</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _repositories?.Clear();
                DbContext.Dispose();
            }
        }
        _disposed = true;
    }

    /// <summary>
    /// 使用跟踪图API附加已分离的实体
    /// </summary>
    /// <param name="rootEntity">根实体</param>
    /// <param name="callback">将对象状态属性转换为实体条目状态的委托</param>
    public void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback)
        => DbContext.ChangeTracker.TrackGraph(rootEntity, callback);

    #endregion
}