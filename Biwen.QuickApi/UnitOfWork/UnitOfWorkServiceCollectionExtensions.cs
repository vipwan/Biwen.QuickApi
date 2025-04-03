// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:55:53 UnitOfWorkServiceCollectionExtensions.cs

using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.UnitOfWork;

[SuppressType]
public static class UnitOfWorkServiceCollectionExtensions
{
    /// <summary>
    /// 添加工作单元
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddScoped<IRepositoryFactory, UnitOfWork<TContext>>();
        // Following has a issue: IUnitOfWork cannot support multiple dbContext/database, 
        // that means cannot call AddUnitOfWork<TContext> multiple times.
        // Solution: check IUnitOfWork whether or null
        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

        return services;
    }

    /// <summary>
    /// 添加工作单元
    /// </summary>
    /// <typeparam name="TContext1"></typeparam>
    /// <typeparam name="TContext2"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddUnitOfWork<TContext1, TContext2>(this IServiceCollection services)
        where TContext1 : DbContext
        where TContext2 : DbContext
    {
        services.AddScoped<IUnitOfWork<TContext1>, UnitOfWork<TContext1>>();
        services.AddScoped<IUnitOfWork<TContext2>, UnitOfWork<TContext2>>();

        return services;
    }

    public static IServiceCollection AddUnitOfWork<TContext1, TContext2, TContext3>(
        this IServiceCollection services)
        where TContext1 : DbContext
        where TContext2 : DbContext
        where TContext3 : DbContext
    {
        services.AddScoped<IUnitOfWork<TContext1>, UnitOfWork<TContext1>>();
        services.AddScoped<IUnitOfWork<TContext2>, UnitOfWork<TContext2>>();
        services.AddScoped<IUnitOfWork<TContext3>, UnitOfWork<TContext3>>();

        return services;
    }

    public static IServiceCollection AddCustomRepository<TEntity, TRepository>(this IServiceCollection services)
        where TEntity : class
        where TRepository : class, IRepository<TEntity>
    {
        services.AddScoped<IRepository<TEntity>, TRepository>();

        return services;
    }
}