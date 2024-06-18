using Biwen.QuickApi.Abstractions.Modular;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.QuickApi.FeatureManagement
{
    internal class FeatureManagementModular : ModularBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            //添加特性管理支持
            services.AddFeatureManagement();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            //添加特性管理中间件用于处理特性
            app.UseMiddleware<EndpointFeatureMiddleware>();

        }
    }
}