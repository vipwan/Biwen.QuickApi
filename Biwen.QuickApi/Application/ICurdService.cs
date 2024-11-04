// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 ICurdService.cs

using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Biwen.QuickApi.Application;

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
    Task<TEntity?> GetAsync(object[] ids);

    /// <summary>
    /// 获取分页数据
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="predicate"></param>
    /// <param name="orderBy"></param>
    /// <param name="include"></param>
    /// <returns></returns>
    Task<IPagedList<TEntity>> GetPagedListAsync(
        int pageIndex = 0,
        int pageSize = 20,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);

}