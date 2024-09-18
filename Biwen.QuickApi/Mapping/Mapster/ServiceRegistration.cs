// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:47:14 ServiceRegistration.cs

using Mapster;
using MapsterMapper;

namespace Biwen.QuickApi.Mapping;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// AddMapster 这是默认的Mapper,会扫描程序集中的配置
    /// </summary>
    /// <param name="services"></param>
    public static void AddMapsterMapper(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.Scan(ASS.AllRequiredAssemblies);
        // 默认（支持覆盖）
        config.Default.NameMatchingStrategy(NameMatchingStrategy.Flexible)
              .PreserveReference(true);

        // 默认（忽略大小写敏感）
        config.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase)
              .PreserveReference(true);

        services.AddActivatedSingleton(_ => config);
        services.AddScoped<IMapper, ServiceMapper>();
    }
}