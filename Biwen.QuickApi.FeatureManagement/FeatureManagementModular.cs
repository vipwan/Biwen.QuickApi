using Biwen.QuickApi.Abstractions.Modular;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Biwen.QuickApi.FeatureManagement
{
    internal class FeatureManagementModular : ModularBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            //添加特性管理支持
            services.AddFeatureManagement();

            //自定义配置QuickApiFeatureManagementOptions
            services.ConfigureQuickApiFeatureManagementOptions(options =>
            {
                options.OnErrorAsync = (async (ctx) =>
                {
                    //返回规范的Result.Problem:
                    await Results.Problem(statusCode: StatusCodes.Status404NotFound).ExecuteAsync(ctx);
                });
            });
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            //添加特性管理中间件用于处理特性
            app.UseMiddleware<EndpointFeatureMiddleware>();

        }
    }
}