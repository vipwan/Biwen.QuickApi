// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:53:18 IScheduleTask.cs

namespace Biwen.QuickApi.Scheduling
{
    /// <summary>
    /// 任务调度接口
    /// </summary>
    public interface IScheduleTask
    {
        /// <summary>
        /// 任务执行
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync();
    }

    /// <summary>
    /// ScheduleTask 抽象类
    /// </summary>
    public abstract class ScheduleTask : IScheduleTask
    {
        public virtual Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 同一时间只能存在一个的任务
    /// </summary>
    public abstract class OnlyOneRunningScheduleTask : ScheduleTask
    {
        /// <summary>
        /// 如果有正在运行的相同任务,打断当前的执行的回调
        /// </summary>
        /// <returns></returns>
        public virtual Task OnAbort()
        {
            return Task.CompletedTask;
        }
    }
}