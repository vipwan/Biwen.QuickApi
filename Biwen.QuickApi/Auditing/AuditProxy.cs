namespace Biwen.QuickApi.Auditing;

using Microsoft.AspNetCore.Http;
using System;
using System.Reflection;
using System.Security.Claims;


internal class AuditProxy<T> : DispatchProxy where T : class
{
    private T? _decorated;
    private IServiceProvider? _serviceProvider;

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        //可以在接口定义中全局配置,也可以在实现类方法中单独配置
        if (_decorated!.GetType()
            .GetMethod(targetMethod.Name, types: args?.Select(x => x?.GetType()!)?.ToArray() ?? [])!
            .GetCustomAttribute<AuditIgnoreAttribute>() is { IgnoreType: IgnoreType.All })
        {
            return targetMethod.Invoke(_decorated, args);
        }

        if (_serviceProvider == null)
        {
            return targetMethod.Invoke(_decorated, args);
        }

        try
        {
            //如果缓存不含的情况下:
            var data = targetMethod.Invoke(_decorated, args);

            //处理审计日志
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var handlers = scope.ServiceProvider.GetService<IEnumerable<IAuditHandler>>();
                var context = scope.ServiceProvider.GetService<IHttpContextAccessor>()?.HttpContext;

                if (handlers is not null)
                {

                    var decoratedType = _decorated.GetType();

                    var methodinfo = decoratedType
                        .GetMethod(targetMethod.Name, types: args?.Select(x => x?.GetType()!)?.ToArray() ?? [])!;

                    var ignoreMeta = methodinfo.GetCustomAttribute<AuditIgnoreAttribute>();

                    var auditInfo = new AuditInfo
                    {
                        ApplicationName = "Biwen.QuickApi",
                        UserId = context?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                        UserName = context?.User.Identity?.Name,
                        BrowserInfo = context?.Request.Headers.UserAgent,
                        ClientIpAddress = context?.Connection.RemoteIpAddress?.ToString(),
                        ClientName = context?.Request.Headers["X-Forwarded-For"],
                        HttpMethod = context?.Request.Method,
                        Url = context?.Request.Path,
                        ActionInfo = new ActionInfo
                        {
                            MethodInfo = methodinfo,
                        },
                        ExtraInfos = new Dictionary<string, object?>()
                        {
                            ["args"] = ignoreMeta?.IgnoreType == IgnoreType.Parameter ? null : args,
                            ["result"] = ignoreMeta?.IgnoreType == IgnoreType.ReturnValue ? null : data
                        }
                    };

                    foreach (var handler in handlers)
                    {
                        handler.Handle(auditInfo).GetAwaiter().GetResult();
                    }
                }
            }
            catch
            {
                //todo:
            }

            return data;
        }
        catch (TargetInvocationException tie)
        {
            throw tie.InnerException ?? tie; // 将原始异常抛出
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// 构造代理
    /// </summary>
    /// <param name="decorated"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static T Create(T decorated, IServiceProvider serviceProvider)
    {
        object proxy = Create<T, AuditProxy<T>>();
        ((AuditProxy<T>)proxy)._decorated = decorated;
        ((AuditProxy<T>)proxy)._serviceProvider = serviceProvider;
        return (T)proxy;
    }
}