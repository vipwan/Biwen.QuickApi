// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:59:55 ClientInfoEnrich.cs

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace Biwen.QuickApi.Logging.Serilog.Enrichers;

/// <summary>
/// 提供客户端信息的日志事件增强器,包括请求地址(RequestUri),客户端IP(ClientIp),客户端代理(Agent),用户名(UserName)
/// 如果满足不了你的需求,可以继承此类,重写Enrich方法
/// </summary>
public class ClientInfoEnrich : ILogEventEnricher
{
    public ClientInfoEnrich(IHttpContextAccessor httpContextAccessor, string forwardHeaderKey = "X-Forwarded-For")
    {
        _forwardHeaderKey = forwardHeaderKey;
        _httpContextAccessor = httpContextAccessor;
    }

    public ClientInfoEnrich(string forwardHeaderKey = "X-Forwarded-For")
    {
        _forwardHeaderKey = forwardHeaderKey;
        _httpContextAccessor = new HttpContextAccessor();
    }

    public ClientInfoEnrich() : this("X-Forwarded-For") { }

    private readonly string _forwardHeaderKey;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public virtual void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(_forwardHeaderKey);
        ArgumentNullException.ThrowIfNull(_httpContextAccessor);

        if (_httpContextAccessor.HttpContext is null)
            return;

        var address = _httpContextAccessor.HttpContext.Request.GetDisplayUrl();
        var agent = _httpContextAccessor.HttpContext.Request.Headers.UserAgent;

        var userName = _httpContextAccessor.HttpContext.User.Identity?.Name;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestUri", address));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Agent", agent));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", userName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientIp", GetIpAddress()));

        string GetIpAddressFromProxy(string proxyFieldIpList)
        {
            ArgumentNullException.ThrowIfNull(proxyFieldIpList);
            var addresses = proxyFieldIpList.Split(',');
            return addresses.Length == 0 ? string.Empty : addresses[0].Trim();
        }

        //客户端IP地址:
        string? GetIpAddress()
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Request?.Headers[_forwardHeaderKey].FirstOrDefault();

            return !string.IsNullOrEmpty(ipAddress)
                ? GetIpAddressFromProxy(ipAddress)
                : _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
    }
}
