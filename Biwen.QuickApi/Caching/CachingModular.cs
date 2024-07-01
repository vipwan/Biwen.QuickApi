using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Biwen.QuickApi.Caching
{
    [CoreModular]
    internal class CachingModular : ModularBase
    {
        /// <summary>
        /// 缓存模块比HttpModular先启动
        /// </summary>
        public override int Order => base.Order - 1;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            services.AddOutputCache();
            services.AddResponseCaching();

            //注入Caching代理
            services.TryAddSingleton(typeof(CachingProxyFactory<>));
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseOutputCache();
            app.UseResponseCaching();
        }
    }
}