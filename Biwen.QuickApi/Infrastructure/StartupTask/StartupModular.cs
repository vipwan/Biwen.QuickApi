using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Biwen.QuickApi.Infrastructure.StartupTask
{
    [CoreModular]
    internal class StartupModular : ModularBase
    {
        /// <summary>
        /// 启动级别较低,不需要最先执行
        /// </summary>
        public override int Order => base.Order + 100;

        private static readonly Type startupType = typeof(IStartupTask);

        public override void ConfigureServices(IServiceCollection services)
        {
            var taskTypes = ASS.InAllRequiredAssemblies.ThatInherit<IStartupTask>()
                .Where(x => x.IsClass && !x.IsAbstract);

            foreach (var taskType in taskTypes)
            {
                services.AddActivatedSingleton(startupType, taskType);
            }
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var tasks = serviceProvider.GetServices<IStartupTask>()?.OrderBy(x => x.Order);
            var logger = serviceProvider.GetRequiredService<ILogger<StartupModular>>();
            if (tasks?.Any() is true)
            {
                foreach (var task in tasks)
                {
                    _ = Task.Run(async () =>
                     {
                         try
                         {
                             await Task.Delay(task.Delay ?? TimeSpan.Zero);
                             await task.ExecuteAsync(default);
                             logger.LogDebug($"StartupTask {task.GetType().Name} executed.");
                         }
                         catch
                         {
                             logger.LogWarning($"StartupTask {task.GetType().Name} failed.");
                         }
                     });
                }
            }
        }
    }
}