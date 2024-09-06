// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:00:37 MiniProfilerModular.cs

using Biwen.QuickApi.Abstractions.Modular;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;

namespace Biwen.QuickApi.MiniProfiler
{

    /// <summary>
    /// MiniProfiler模块
    /// </summary>
    /// <param name="configuration"></param>
    internal sealed class MiniProfilerModular(IConfiguration configuration) : ModularBase
    {
        /// <summary>
        /// 根据配置项判断是否启用
        /// </summary>
        public override Func<bool> IsEnable => () =>
        {
            var flag = configuration.GetValue<bool?>($"{WrapMiniProfilerOptions.Key}:Enabled");
            return flag is true;
        };

        /// <summary>
        /// 注册级别较低
        /// </summary>
        public override int Order => base.Order + 100;


        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseMiniProfiler();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            //配置MiniProfiler
            services.Configure<MiniProfilerOptions>(configuration.GetSection(WrapMiniProfilerOptions.Key));

            //WrapMiniProfilerOptions
            services.Configure<WrapMiniProfilerOptions>(configuration.GetSection(WrapMiniProfilerOptions.Key));

            services
                .AddMiniProfiler()
                .AddEntityFramework();//提供EFCore支持
        }
    }
}