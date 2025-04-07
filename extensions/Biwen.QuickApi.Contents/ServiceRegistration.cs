// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 15:04:37 ServiceRegistration.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.Contents.FieldTypes;
using Biwen.QuickApi.Contents.Schema;
using Biwen.QuickApi.Contents.Services;
using Biwen.QuickApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.Contents;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// 注册内容模块
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options">配置</param>
    /// <returns></returns>
    public static IServiceCollection AddBiwenContents<TDbContext>(this IServiceCollection services,
        Action<BiwenContentOptions>? options = null)
        where TDbContext : DbContext, IContentDbContext
    {
        // Options
        services.AddOptions<BiwenContentOptions>().Configure(x => { options?.Invoke(x); });

        // 注册所有字段类型
        services.AddSingleton<IFieldType, TextFieldType>();
        services.AddSingleton<IFieldType, BooleanFieldType>();
        services.AddSingleton<IFieldType, IntegerFieldType>();
        services.AddSingleton<IFieldType, UrlFieldType>();
        services.AddSingleton<IFieldType, ColorFieldType>();
        services.AddSingleton<IFieldType, TextAreaFieldType>();
        services.AddSingleton<IFieldType, MarkdownFieldType>();
        services.AddSingleton<IFieldType, NumberFieldType>();

        services.AddSingleton<IFieldType, TimeFieldType>();
        services.AddSingleton<IFieldType, DateTimeFieldType>();
        services.AddSingleton<IFieldType, ImageFieldType>();
        services.AddSingleton<IFieldType, FileFieldType>();

        //单选
        services.AddSingleton<IFieldType, OptionsFieldType<int>>();//选项.默认枚举存储的类型为int
        //复选
        services.AddSingleton<IFieldType, OptionsMultiFieldType<int>>();//选项.默认枚举存储的类型为int

        // 注册ArrayFieldType
        services.AddSingleton<IFieldType, ArrayFieldType>();

        // 注册字段管理器
        services.AddSingleton<ContentFieldManager>();

        //注册Cache
        services.AddMemoryCache();

        // Schema生成器
        services.AddSingleton<IContentSchemaGenerator, XRenderSchemaGenerator>();

        // 注册文件类型提供器
        services.AddSingleton<ContentSerializer>();

        // 注册内容仓储上下文
        services.AddScoped<IContentDbContext, TDbContext>();
        // 注册内容仓储
        services.AddScoped<IContentRepository, ContentRepository>();

        // 注册内容审计日志服务
        services.AddScoped<IContentAuditLogService, ContentAuditLogService>();
        // 注册内容版本服务
        services.AddScoped<IContentVersionService, ContentVersionService>();

        // 注册GroupRouteBuilder
        services.AddQuickApiGroupRouteBuilder<ContentsGroupRouteBuilder>();

        return services;
    }
}