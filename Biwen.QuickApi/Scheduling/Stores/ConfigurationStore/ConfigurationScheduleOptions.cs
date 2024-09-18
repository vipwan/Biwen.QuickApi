// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:52:41 ConfigurationScheduleOptions.cs

namespace Biwen.QuickApi.Scheduling.Stores.ConfigurationStore;

[Obsolete]
internal class ConfigurationScheduleOptions
{
    public string ScheduleType { get; set; } = null!;
    public string Cron { get; set; } = null!;
    public string? Description { get; set; }

    public bool IsAsync { get; set; } = false;

    public bool IsStartOnInit { get; set; } = false;

}