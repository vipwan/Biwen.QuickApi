// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:55:05 AutoEventDbContext.cs

using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.UnitOfWork;

/// <summary>
/// 自动广播Entity的增删改事件的DbContext
/// </summary>
public abstract class AutoEventDbContext<TDbContext> : DbContext where TDbContext : DbContext
{
    public AutoEventDbContext(DbContextOptions<TDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //实现自动广播Entity的增删改事件
        optionsBuilder.AddInterceptors(new AutoEventInterceptor());
    }
}