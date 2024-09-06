// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 ModularBase.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Biwen.QuickApi.Abstractions.Modular
{
    /// <summary>
    /// 模块基类
    /// </summary>
    public abstract class ModularBase : IStartup
    {
        /// <inheritdoc />
        public virtual int Order { get; } = 0;

        public virtual Func<bool> IsEnable => () => true;

        /// <inheritdoc />
        public virtual void ConfigureServices(IServiceCollection services)
        {
        }

        /// <inheritdoc />
        public virtual void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}