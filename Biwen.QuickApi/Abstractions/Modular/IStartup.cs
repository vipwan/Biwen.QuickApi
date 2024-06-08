using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Biwen.QuickApi.Abstractions.Modular
{
    /// <summary>
    /// An implementation of this interface is used to initialize the services and HTTP request
    /// pipeline of a module.
    /// </summary>
    internal interface IStartup
    {
        /// <summary>
        /// 是否启用 默认:启用,动态判断请注入:IServiceProvider
        /// </summary>
        Func<bool> IsEnable { get; }

        /// <summary>
        /// 首先加载前置模块[PreModular],然后再按Order排序 默认:0
        /// </summary>
        int Order { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940.
        /// </summary>
        /// <param name="services">The collection of service descriptors.</param>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="routes"></param>
        /// <param name="serviceProvider"></param>
        void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider);
    }
}