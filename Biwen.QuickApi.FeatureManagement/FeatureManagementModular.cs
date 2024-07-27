using Biwen.QuickApi.Abstractions.Modular;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Biwen.QuickApi.FeatureManagement
{
    internal class FeatureManagementModular : ModularBase
    {
        /// <summary>
        /// 排序为0
        /// </summary>
        public override int Order => Constants.Order;


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