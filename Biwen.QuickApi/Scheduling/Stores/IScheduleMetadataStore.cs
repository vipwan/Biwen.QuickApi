// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:52:59 IScheduleMetadataStore.cs

namespace Biwen.QuickApi.Scheduling.Stores;

/// <summary>
/// 存储调度任务Metadata的接口,可以自行扩展配置文件或者数据库存储
/// </summary>
public interface IScheduleMetadataStore
{
    /// <summary>
    /// 获取所有配置任务的Metadata
    /// </summary>
    /// <returns></returns>
    Task<ScheduleTaskMetadata[]> GetAllAsync();
}