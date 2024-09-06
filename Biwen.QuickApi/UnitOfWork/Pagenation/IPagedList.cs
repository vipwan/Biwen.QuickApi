// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:54:46 IPagedList.cs

namespace Biwen.QuickApi.UnitOfWork.Pagenation;

/// <summary>
/// 提供查询分页的接口 of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type for paging.</typeparam>
public interface IPagedList<T>
{
    /// <summary>
    /// 获取索引开始值
    /// </summary>
    /// <value>The index start value.</value>
    int IndexFrom { get; }

    /// <summary>
    /// 当前页码,从0开始
    /// </summary>
    int PageIndex { get; }

    /// <summary>
    /// 每页显示的数量
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// 获取总数 of <typeparamref name="T"/>
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// 获取总页数
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// 获取当前页的数据 of <typeparamref name="T"/>
    /// </summary>
    IList<T> Items { get; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    /// <value>The has previous page.</value>
    bool HasPreviousPage { get; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    /// <value>The has next page.</value>
    bool HasNextPage { get; }
}