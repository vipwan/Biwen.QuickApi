// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 ConsoleAuditHandler.cs

using Biwen.QuickApi.Auditing.Abstractions;

namespace Biwen.QuickApi.Auditing.Internal;

internal class ConsoleAuditHandler(ILogger<ConsoleAuditHandler> logger) : IAuditHandler
{
    public Task HandleAsync(AuditInfo auditInfo)
    {
        //仅针对Public方法拦截打印
        if (auditInfo.ActionInfo?.MethodInfo?.IsPublic is true)
        {
            logger.LogTrace("AuditInfo: {@auditInfo}", auditInfo);
        }

        if (auditInfo.IsQuickApi)
        {
            logger.LogInformation("QuickApi: {@Url}", auditInfo.Url);
        }

        return Task.CompletedTask;
    }
}