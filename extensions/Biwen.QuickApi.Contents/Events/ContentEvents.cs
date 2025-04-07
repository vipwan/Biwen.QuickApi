// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan

using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.Events;

namespace Biwen.QuickApi.Contents.Events;

/// <summary>
/// 内容创建事件
/// </summary>
/// <param name="Content">创建的内容</param>
public record ContentCreatedEvent(Content Content) : IEvent;

/// <summary>
/// 内容更新事件
/// </summary>
/// <param name="Content">更新的内容</param>
/// <param name="PreviousVersion">旧版本内容</param>
/// <param name="IsRollback">是否回滚</param>
/// <param name="RollbackVersion">回滚版本</param>
public record ContentUpdatedEvent(Content Content, string PreviousVersion, bool IsRollback = false, int RollbackVersion = 0) : IEvent;

/// <summary>
/// 内容删除事件
/// </summary>
/// <param name="ContentId">内容ID</param>
public record ContentDeletedEvent(Guid ContentId) : IEvent;

/// <summary>
/// 内容状态更改事件
/// </summary>
/// <param name="Content">内容</param>
/// <param name="PreviousStatus">旧状态</param>
/// <param name="NewStatus">新状态</param>
public record ContentStatusChangedEvent(Content Content, ContentStatus PreviousStatus, ContentStatus NewStatus) : IEvent;