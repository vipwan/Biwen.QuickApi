using Biwen.QuickApi.Infrastructure;
using Biwen.QuickApi.Logging.Serilog.Enrichers;
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
        /// <param name="withClientInfo">是否包含客户端信息的Enricher</param>
        /// <returns></returns>
        public static IHostBuilder UseSerilogFromConfiguration(this IHostBuilder builder, bool withClientInfo = true)
        {
            builder.UseSerilog((context, cfg) =>
             {
                 cfg.ReadFrom.Configuration(context.Configuration)
                   .Enrich.When((e) => withClientInfo, c =>
                   {
                       c.With<ClientInfoEnrich>();
                   })
                   .Enrich.FromLogContext();
             });

            return builder;
        }
    }
}