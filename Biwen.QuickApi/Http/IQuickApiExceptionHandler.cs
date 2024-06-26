﻿namespace Biwen.QuickApi.Http;

/// <summary>
/// 全局QuickApi异常处理
/// </summary>
public interface IQuickApiExceptionHandler
{
    Task HandleAsync(Exception exception);
}