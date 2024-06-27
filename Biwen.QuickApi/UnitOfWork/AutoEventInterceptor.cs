using Biwen.QuickApi.Service.EntityEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Biwen.QuickApi.UnitOfWork;

/// <summary>
/// 拦截器,自动广播Entity的增删改事件
/// </summary>
internal class AutoEventInterceptor : ISaveChangesInterceptor
{
    /// <summary>
    /// 拦截器,自动广播Entity的增删改事件
    /// </summary>
    public AutoEventInterceptor() { }

    #region SavedChanges
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context!;


        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity.GetType().GetCustomAttribute<AutoEventIgnoreAttribute>() is { })
                continue;

            _ = entry.State switch
            {
                EntityState.Deleted => _publishDeletedAsync.MakeGenericMethod(entry.Entity.GetType()).Invoke(null, [entry.Entity]),
                EntityState.Modified => _publishUpdatedAsync.MakeGenericMethod(entry.Entity.GetType()).Invoke(null, [entry.Entity]),
                EntityState.Added => _publishAddedAsync.MakeGenericMethod(entry.Entity.GetType()).Invoke(null, [entry.Entity]),
                _ => null
            };
        }

        return result;
    }

    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context!;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity.GetType().GetCustomAttribute<AutoEventIgnoreAttribute>() is { })
                continue;

            _ = entry.State switch
            {
                EntityState.Deleted => _publishDeletedAsync.MakeGenericMethod(entry.Entity.GetType()).Invoke(null, [entry.Entity]),
                EntityState.Modified => _publishUpdatedAsync.MakeGenericMethod(entry.Entity.GetType()).Invoke(null, [entry.Entity]),
                EntityState.Added => _publishAddedAsync.MakeGenericMethod(entry.Entity.GetType()).Invoke(null, [entry.Entity]),
                _ => null
            };
        }

        await Task.CompletedTask;
        return result;
    }

    private static readonly MethodInfo _publishAddedAsync = typeof(EntityExtensions).GetMethod(nameof(EntityExtensions.PublishAddedAsync))!;
    private static readonly MethodInfo _publishUpdatedAsync = typeof(EntityExtensions).GetMethod(nameof(EntityExtensions.PublishUpdatedAsync))!;
    private static readonly MethodInfo _publishDeletedAsync = typeof(EntityExtensions).GetMethod(nameof(EntityExtensions.PublishDeletedAsync))!;

    #endregion
}
