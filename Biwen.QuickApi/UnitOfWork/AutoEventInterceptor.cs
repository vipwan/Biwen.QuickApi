using Biwen.QuickApi.Application.EntityEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        ProcessEntityEvents(eventData.Context!.ChangeTracker.Entries());
        return result;
    }

    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ProcessEntityEvents(eventData.Context!.ChangeTracker.Entries());
        await Task.CompletedTask;
        return result;
    }

    private void ProcessEntityEvents(IEnumerable<EntityEntry> entries)
    {
        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType();
            if (entityType.GetCustomAttribute<AutoEventIgnoreAttribute>() is { })
                continue;

            _ = entry.State switch
            {
                EntityState.Deleted => _publishDeletedAsync.MakeGenericMethod(entityType).Invoke(null, [entry.Entity]),
                EntityState.Modified => _publishUpdatedAsync.MakeGenericMethod(entityType).Invoke(null, [entry.Entity]),
                EntityState.Added => _publishAddedAsync.MakeGenericMethod(entityType).Invoke(null, [entry.Entity]),
                _ => null
            };
        }
    }

    #region private

    private static readonly MethodInfo _publishAddedAsync = typeof(EntityExtensions).GetMethod(nameof(EntityExtensions.PublishAddedAsync))!;
    private static readonly MethodInfo _publishUpdatedAsync = typeof(EntityExtensions).GetMethod(nameof(EntityExtensions.PublishUpdatedAsync))!;
    private static readonly MethodInfo _publishDeletedAsync = typeof(EntityExtensions).GetMethod(nameof(EntityExtensions.PublishDeletedAsync))!;

    #endregion

    #endregion
}
