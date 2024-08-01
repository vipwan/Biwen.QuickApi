using Biwen.QuickApi.Infrastructure;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Biwen.QuickApi.Logging
{


    [SuppressType]
    public static class ServiceRegistration
    {
        /// <summary>
        /// 默认使用配置文件中的Serilog配置
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostBuilder UseSerilogFromConfiguration(this IHostBuilder builder)
        {
            builder.UseSerilog((context, cfg) =>
             {
                 cfg.ReadFrom
                   .Configuration(context.Configuration)
                   .Enrich
                   .FromLogContext();
             });

            return builder;
        }
    }
}