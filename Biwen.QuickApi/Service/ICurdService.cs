using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Biwen.QuickApi.Service;

/// <summary>
/// <paramref name="TEntity"/> 增删改查接口.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ICurdService<TEntity> where TEntity : class
{
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task DeleteAsync(TEntity entity);

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="ids">可能存在双重主键</param>
    /// <returns></returns>
    Task<TEntity> GetAsync(object[] ids);

    /// <summary>
    /// 获取分页数据
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="predicate"></param>
    /// <param name="orderBy"></param>
    /// <param name="include"></param>
    /// <returns></returns>
    Task<IPagedList<TEntity>> GetPagedList(
        int pageIndex = 0,
        int pageSize = 20,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);

}