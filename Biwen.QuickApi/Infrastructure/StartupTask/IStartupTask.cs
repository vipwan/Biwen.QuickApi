// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:46:19 IStartupTask.cs

namespace Biwen.QuickApi.Infrastructure.StartupTask
{
    /// <summary>
    /// 启动任务,系统启动后只执行一次的任务
    /// </summary>
    public interface IStartupTask
    {
        /// <summary>
        /// 启动系统后执行的任务
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken ct);

        /// <summary>
        /// 执行顺序
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 启动延迟 默认为null,不延迟
        /// </summary>
        TimeSpan? Delay { get; }

    }

    /// <summary>
    /// 启动任务,系统启动后只执行一次的任务
    /// </summary>
    public abstract class StartupTaskBase : IStartupTask
    {
        public virtual int Order => 0;

        public virtual TimeSpan? Delay => null;

        public abstract Task ExecuteAsync(CancellationToken ct);
    }

}