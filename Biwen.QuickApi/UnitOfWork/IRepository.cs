// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi 作者: 万雅虎 Github: https://github.com/vipwan
// 修改日期: 2025-04-03 IRepository.cs

using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Biwen.QuickApi.UnitOfWork;

/// <summary>
/// EfCore的泛型仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    #region GetPagedList

    /// <summary>
    /// 根据条件表达式、排序方式和分页信息获取 <see cref="IPagedList{T}"/> 结果集。此方法默认禁用实体跟踪。
    /// </summary>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="orderBy">用于排序元素的函数</param>
    /// <param name="include">用于包含导航属性的函数</param>
    /// <param name="pageIndex">页索引，从0开始</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <param name="ignoreQueryFilters">是否忽略查询筛选器</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动包含</param>
    /// <returns>包含满足 <paramref name="predicate"/> 指定条件的元素的 <see cref="IPagedList{TEntity}"/> 实例</returns>
    /// <remarks>此方法默认禁用实体跟踪查询</remarks>
    IPagedList<TEntity> GetPagedList(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int pageIndex = 0,
        int pageSize = 20,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false);

    /// <summary>
    /// 分页查询 <see cref="IPagedList{TEntity}"/> 
    /// </summary>
    /// <param name="predicate">条件表达式</param>
    /// <param name="orderBy">排序表达式</param>
    /// <param name="include">包含表达式</param>
    /// <param name="pageIndex">页码 默认:0,表示第一页</param>
    /// <param name="pageSize">分页尺寸 默认:20</param>
    /// <param name="disableTracking">是否禁用追踪,默认:True</param>
    /// <param name="ignoreQueryFilters">是否忽略查询Filter,默认:False</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动Includes,默认:False</param>
    /// <param name="cancellationToken">
    ///     用于观察任务完成的 <see cref="CancellationToken"/>
    /// </param>
    /// <returns>包含满足 <paramref name="predicate"/> 指定条件的元素的 <see cref="IPagedList{TEntity}"/> 实例</returns>
    /// <remarks>请注意当前方法 默认禁止了EntityTracking</remarks>
    Task<IPagedList<TEntity>> GetPagedListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int pageIndex = 0,
        int pageSize = 20,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false,
        CancellationToken cancellationToken = default
        );

    /// <summary>
    /// 根据条件表达式、排序方式和分页信息获取投影后的 <see cref="IPagedList{TResult}"/> 结果集。此方法默认禁用实体跟踪。
    /// </summary>
    /// <param name="selector">用于投影的选择器</param>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="orderBy">用于排序元素的函数</param>
    /// <param name="include">用于包含导航属性的函数</param>
    /// <param name="pageIndex">页索引，从0开始</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <param name="ignoreQueryFilters">是否忽略查询筛选器</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动包含</param>
    /// <returns>包含满足 <paramref name="predicate"/> 指定条件的元素的 <see cref="IPagedList{TResult}"/> 实例</returns>
    /// <remarks>此方法默认禁用实体跟踪查询</remarks>
    IPagedList<TResult> GetPagedList<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int pageIndex = 0,
        int pageSize = 20,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false) where TResult : class;

    /// <summary>
    /// 异步根据条件表达式、排序方式和分页信息获取投影后的 <see cref="IPagedList{TResult}"/> 结果集。此方法默认禁用实体跟踪。
    /// </summary>
    /// <param name="selector">用于投影的选择器</param>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="orderBy">用于排序元素的函数</param>
    /// <param name="include">用于包含导航属性的函数</param>
    /// <param name="pageIndex">页索引，从0开始</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <param name="cancellationToken">用于观察任务完成的 <see cref="CancellationToken"/></param>
    /// <param name="ignoreQueryFilters">是否忽略查询筛选器</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动包含</param>
    /// <returns>包含满足 <paramref name="predicate"/> 指定条件的元素的 <see cref="IPagedList{TResult}"/> 实例</returns>
    /// <remarks>此方法默认禁用实体跟踪查询</remarks>
    Task<IPagedList<TResult>> GetPagedListAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int pageIndex = 0,
        int pageSize = 20,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false,
        CancellationToken cancellationToken = default) where TResult : class;

    #endregion

    #region GetFirstOrDefault

    /// <summary>
    /// 根据条件表达式、排序方式和导航属性包含获取第一个或默认实体。此方法默认为只读、非跟踪查询。
    /// </summary>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="orderBy">用于排序元素的函数</param>
    /// <param name="include">用于包含导航属性的函数</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <param name="ignoreQueryFilters">是否忽略查询筛选器</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动包含</param>
    /// <returns>满足条件的第一个元素，如果没有则返回默认值</returns>
    /// <remarks>此方法默认为只读、非跟踪查询</remarks>
    TEntity? GetFirstOrDefault(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false);

    /// <summary>
    /// 根据条件表达式、排序方式和导航属性包含获取投影后的第一个或默认实体。此方法默认为只读、非跟踪查询。
    /// </summary>
    /// <param name="selector">用于投影的选择器</param>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="orderBy">用于排序元素的函数</param>
    /// <param name="include">用于包含导航属性的函数</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <param name="ignoreQueryFilters">是否忽略查询筛选器</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动包含</param>
    /// <returns>满足条件的第一个投影元素，如果没有则返回默认值</returns>
    /// <remarks>此方法默认为只读、非跟踪查询</remarks>
    TResult? GetFirstOrDefault<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false);

    /// <summary>
    /// 异步根据条件表达式、排序方式和导航属性包含获取第一个或默认实体。此方法默认为只读、非跟踪查询。
    /// </summary>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="orderBy">用于排序元素的函数</param>
    /// <param name="include">用于包含导航属性的函数</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <param name="ignoreQueryFilters">是否忽略查询筛选器</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动包含</param>
    /// <returns>表示异步操作的任务，返回满足条件的第一个元素，如果没有则返回默认值</returns>
    /// <remarks>此方法默认为只读、非跟踪查询</remarks>
    Task<TEntity?> GetFirstOrDefaultAsync
    (
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false);

    #endregion

    #region Find

    /// <summary>
    /// 使用给定的主键值查找实体。如果找到，则将其附加到上下文并返回。如果未找到任何实体，则返回 null。
    /// </summary>
    /// <param name="keyValues">要查找的实体的主键值</param>
    /// <returns>找到的实体，如果未找到则返回 null</returns>
    TEntity? Find(params object[] keyValues);

    /// <summary>
    /// 异步使用给定的主键值查找实体。如果找到，则将其附加到上下文并返回。如果未找到任何实体，则返回 null。
    /// </summary>
    /// <param name="keyValues">要查找的实体的主键值</param>
    /// <returns>表示异步查找操作的任务，任务结果包含找到的实体，如果未找到则为 null</returns>
    ValueTask<TEntity?> FindAsync(params object[] keyValues);

    /// <summary>
    /// 异步使用给定的主键值查找实体。如果找到，则将其附加到上下文并返回。如果未找到任何实体，则返回 null。
    /// </summary>
    /// <param name="keyValues">要查找的实体的主键值</param>
    /// <param name="cancellationToken">用于观察任务完成的 <see cref="CancellationToken"/></param>
    /// <returns>表示异步查找操作的任务，任务结果包含找到的实体，如果未找到则为 null</returns>
    ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken cancellationToken);

    #endregion

    #region GetAll

    /// <summary>
    /// 获取所有实体。不推荐使用此方法
    /// </summary>
    /// <returns><see cref="IQueryable{TEntity}"/> 类型的结果</returns>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    IQueryable<TEntity> GetAll(bool disableTracking = true);

    /// <summary>
    /// 获取所有投影后的实体。不推荐使用此方法
    /// </summary>
    /// <param name="selector">用于投影的选择器</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <returns><see cref="IQueryable{TResult}"/> 类型的结果</returns>
    IQueryable<TResult> GetAll<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        bool disableTracking = true);

    /// <summary>
    /// 根据条件获取所有投影后的实体。不推荐使用此方法
    /// </summary>
    /// <param name="selector">用于投影的选择器</param>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <returns><see cref="IQueryable{TResult}"/> 类型的结果</returns>
    IQueryable<TResult> GetAll<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        bool disableTracking = true);

    /// <summary>
    /// 根据条件获取所有实体。不推荐使用此方法
    /// </summary>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="orderBy">用于排序元素的函数</param>
    /// <param name="include">用于包含导航属性的函数</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <param name="ignoreQueryFilters">是否忽略查询筛选器</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动包含</param>
    /// <returns>满足条件的查询结果</returns>
    /// <remarks>此方法默认为只读、非跟踪查询</remarks>
    IQueryable<TEntity> GetAll(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false);

    /// <summary>
    /// 根据条件获取所有投影后的实体。不推荐使用此方法
    /// </summary>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="selector">用于投影的选择器</param>
    /// <param name="orderBy">用于排序元素的函数</param>
    /// <param name="include">用于包含导航属性的函数</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <param name="ignoreQueryFilters">是否忽略查询筛选器</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动包含</param>
    /// <returns>满足条件的查询结果</returns>
    /// <remarks>此方法默认为只读、非跟踪查询</remarks>
    IQueryable<TResult> GetAll<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false);

    /// <summary>
    /// 异步获取所有实体。不推荐使用此方法
    /// </summary>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <returns>实体列表集合</returns>
    Task<IList<TEntity>> GetAllAsync(bool disableTracking = true);

    /// <summary>
    /// 异步获取所有投影后的实体。不推荐使用此方法
    /// </summary>
    /// <param name="selector">用于投影的选择器</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <returns>投影后的实体列表集合</returns>
    Task<IList<TResult>> GetAllAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        bool disableTracking = true);

    /// <summary>
    /// 异步根据条件获取所有实体。不推荐使用此方法
    /// </summary>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="orderBy">用于排序元素的函数</param>
    /// <param name="include">用于包含导航属性的函数</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <param name="ignoreQueryFilters">是否忽略查询筛选器</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动包含</param>
    /// <returns>满足条件的实体列表集合</returns>
    /// <remarks>此方法默认为只读、非跟踪查询</remarks>
    Task<IList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false);

    /// <summary>
    /// 异步根据条件获取所有投影后的实体。不推荐使用此方法
    /// </summary>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <param name="selector">用于投影的选择器</param>
    /// <param name="orderBy">用于排序元素的函数</param>
    /// <param name="include">用于包含导航属性的函数</param>
    /// <param name="disableTracking">是否禁用更改跟踪，默认为 <c>true</c></param>
    /// <param name="ignoreQueryFilters">是否忽略查询筛选器</param>
    /// <param name="ignoreAutoIncludes">是否忽略自动包含</param>
    /// <returns>满足条件的投影后实体列表集合</returns>
    /// <remarks>此方法默认为只读、非跟踪查询</remarks>
    Task<IList<TResult>> GetAllAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false);

    #endregion

    #region Insert

    /// <summary>
    /// 同步插入新实体
    /// </summary>
    /// <param name="entity">要插入的实体</param>
    /// <returns>插入后的实体</returns>
    TEntity Insert(TEntity entity);

    /// <summary>
    /// 同步插入多个实体
    /// </summary>
    /// <param name="entities">要插入的实体数组</param>
    void Insert(params TEntity[] entities);

    /// <summary>
    /// 同步插入多个实体
    /// </summary>
    /// <param name="entities">要插入的实体集合</param>
    void Insert(IEnumerable<TEntity> entities);

    /// <summary>
    /// 异步插入新实体
    /// </summary>
    /// <param name="entity">要插入的实体</param>
    /// <param name="cancellationToken">用于观察任务完成的 <see cref="CancellationToken"/></param>
    /// <returns>表示异步插入操作的任务</returns>
    ValueTask<EntityEntry<TEntity>> InsertAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));

    /// <summary>
    /// 异步插入多个实体
    /// </summary>
    /// <param name="entities">要插入的实体数组</param>
    /// <returns>表示异步插入操作的任务</returns>
    Task InsertAsync(params TEntity[] entities);

    /// <summary>
    /// 异步插入多个实体
    /// </summary>
    /// <param name="entities">要插入的实体集合</param>
    /// <param name="cancellationToken">用于观察任务完成的 <see cref="CancellationToken"/></param>
    /// <returns>表示异步插入操作的任务</returns>
    Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken));

    #endregion

    #region Update

    /// <summary>
    /// 更新指定实体
    /// </summary>
    /// <param name="entity">要更新的实体</param>
    void Update(TEntity entity);

    /// <summary>
    /// 更新多个指定实体
    /// </summary>
    /// <param name="entities">要更新的实体数组</param>
    void Update(params TEntity[] entities);

    /// <summary>
    /// 更新多个指定实体
    /// </summary>
    /// <param name="entities">要更新的实体集合</param>
    void Update(IEnumerable<TEntity> entities);

    /// <summary>
    /// 批量更新符合LINQ查询的数据库行
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此操作立即针对数据库执行，而不是等到调用 <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> 时才执行。
    /// 它也不会以任何方式与EF变更跟踪器交互：调用此操作时正在跟踪的实体实例不会被考虑，也不会更新以反映更改。
    /// </para>
    /// <para>
    /// 有关更多信息和示例，请参阅 <see href="https://aka.ms/efcore-docs-bulk-operations">使用EF Core执行批量操作</see>
    /// </para>
    /// </remarks>
    /// <param name="predicate">属性更新表达式</param>
    /// <returns>数据库中更新的总行数</returns>
    int ExecuteUpdate(Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> predicate);

    /// <summary>
    /// 异步批量更新符合LINQ查询的数据库行
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此操作立即针对数据库执行，而不是等到调用 <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> 时才执行。
    /// 它也不会以任何方式与EF变更跟踪器交互：调用此操作时正在跟踪的实体实例不会被考虑，也不会更新以反映更改。
    /// </para>
    /// <para>
    /// 有关更多信息和示例，请参阅 <see href="https://aka.ms/efcore-docs-bulk-operations">使用EF Core执行批量操作</see>
    /// </para>
    /// </remarks>
    /// <param name="predicate">属性更新表达式</param>
    /// <param name="cancellationToken">用于观察任务完成的 <see cref="T:System.Threading.CancellationToken"/></param>
    /// <returns>数据库中更新的总行数</returns>
    Task<int> ExecuteUpdateAsync(
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> predicate,
        CancellationToken cancellationToken);

    #endregion

    #region Delete

    /// <summary>
    /// 根据主键删除实体
    /// </summary>
    /// <param name="id">主键值</param>
    void Delete(object id);

    /// <summary>
    /// 删除指定实体
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    void Delete(TEntity entity);

    /// <summary>
    /// 删除多个指定实体
    /// </summary>
    /// <param name="entities">要删除的实体数组</param>
    void Delete(params TEntity[] entities);

    /// <summary>
    /// 删除多个指定实体
    /// </summary>
    /// <param name="entities">要删除的实体集合</param>
    void Delete(IEnumerable<TEntity> entities);

    /// <summary>
    /// 批量删除符合LINQ查询的所有数据库行
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此操作立即针对数据库执行，而不是等到调用 <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> 时才执行。
    /// 它也不会以任何方式与EF变更跟踪器交互：调用此操作时正在跟踪的实体实例不会被考虑，也不会更新以反映更改。
    /// </para>
    /// <para>
    /// 有关更多信息和示例，请参阅 <see href="https://aka.ms/efcore-docs-bulk-operations">使用EF Core执行批量操作</see>
    /// </para>
    /// </remarks>
    /// <returns>数据库中删除的总行数</returns>
    int ExecuteDelete();

    /// <summary>
    /// 异步批量删除符合LINQ查询的所有数据库行
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此操作立即针对数据库执行，而不是等到调用 <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> 时才执行。
    /// 它也不会以任何方式与EF变更跟踪器交互：调用此操作时正在跟踪的实体实例不会被考虑，也不会更新以反映更改。
    /// </para>
    /// <para>
    /// 有关更多信息和示例，请参阅 <see href="https://aka.ms/efcore-docs-bulk-operations">使用EF Core执行批量操作</see>
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">用于观察任务完成的 <see cref="T:System.Threading.CancellationToken"/></param>
    /// <returns>数据库中删除的总行数</returns>
    Task<int> ExecuteDeleteAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Count

    /// <summary>
    /// 根据条件获取记录数量
    /// </summary>
    /// <param name="predicate">条件表达式</param>
    /// <returns>记录数量</returns>
    int Count(Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// 异步根据条件获取记录数量
    /// </summary>
    /// <param name="predicate">条件表达式</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>记录数量</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据条件获取长整型记录数量
    /// </summary>
    /// <param name="predicate">条件表达式</param>
    /// <returns>记录数量</returns>
    long LongCount(Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// 异步根据条件获取长整型记录数量
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="predicate">条件表达式</param>
    /// <returns>记录数量</returns>
    Task<long> LongCountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    #endregion

    #region Exists

    /// <summary>
    /// 根据条件判断记录是否存在
    /// </summary>
    /// <param name="predicate">条件表达式</param>
    /// <returns>如果存在则返回true，否则返回false</returns>
    bool Exists(Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// 异步根据条件判断记录是否存在
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="selector">条件表达式</param>
    /// <returns>如果存在则返回true，否则返回false</returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? selector = null, CancellationToken cancellationToken = default);

    #endregion

    #region Aggregations

    /// <summary>
    /// 根据条件获取最大值
    /// </summary>
    /// <param name="predicate">条件表达式</param>
    /// <param name="selector">选择器表达式</param>
    /// <returns>最大值</returns>
    T? Max<T>(Expression<Func<TEntity, T>> selector, Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// 异步根据条件获取最大值
    /// </summary>
    /// <param name="selector">选择器表达式</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="predicate">条件表达式</param>
    /// <returns>最大值</returns>
    Task<T> MaxAsync<T>(
        Expression<Func<TEntity, T>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据条件获取最小值
    /// </summary>
    /// <param name="selector">选择器表达式</param>
    /// <param name="predicate">条件表达式</param>
    /// <returns>最小值</returns>
    T? Min<T>(Expression<Func<TEntity, T>> selector,
        Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// 异步根据条件获取最小值
    /// </summary>
    /// <param name="predicate">条件表达式</param>
    /// <param name="selector">选择器表达式</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>最小值</returns>
    Task<T> MinAsync<T>(
        Expression<Func<TEntity, T>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据条件获取平均值
    /// </summary>
    /// <param name="selector">选择器表达式</param>
    /// <param name="predicate">条件表达式</param>
    /// <returns>平均值</returns>
    decimal Average(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// 异步根据条件获取平均值
    /// </summary>
    /// <param name="selector">选择器表达式</param>
    /// <param name="predicate">条件表达式</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>平均值</returns>
    Task<decimal> AverageAsync(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据条件获取求和值
    /// </summary>
    /// <param name="selector">选择器表达式</param>
    /// <param name="predicate">条件表达式</param>
    /// <returns>求和值</returns>
    decimal Sum(Expression<Func<TEntity, decimal>> selector, Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// 异步根据条件获取求和值
    /// </summary>
    /// <param name="selector">选择器表达式</param>
    /// <param name="predicate">条件表达式</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>求和值</returns>
    Task<decimal> SumAsync(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Other

    /// <summary>
    /// 为Web API的PATCH方法更改实体状态
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="state">实体状态</param>
    void ChangeEntityState(TEntity entity, EntityState state);

    /// <summary>
    /// 更改表名。这要求表位于同一数据库中。
    /// </summary>
    /// <param name="table">表名</param>
    /// <remarks>
    /// 这仅用于支持同一模型中的多个表。这要求表位于同一数据库中。
    /// </remarks>
    void ChangeTable(string table);


    /// <summary>
    /// 使用原始SQL查询获取指定实体类型的数据
    /// </summary>
    /// <param name="sql">原始SQL语句</param>
    /// <param name="parameters">参数</param>
    /// <returns>包含满足原始SQL指定条件的元素的 <see cref="IQueryable{TEntity}"/> 对象</returns>
    IQueryable<TEntity> FromSql(string sql, params object[] parameters);

    #endregion
}
