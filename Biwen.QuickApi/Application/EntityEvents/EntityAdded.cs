// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 EntityAdded.cs

namespace Biwen.QuickApi.Application.EntityEvents;

/// <summary>
/// 实体添加事件
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EntityAdded<TEntity> : IEntityEvent where TEntity : class
{
    public TEntity Entity { get; }

    public EntityAdded(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Entity = entity;
    }
}
