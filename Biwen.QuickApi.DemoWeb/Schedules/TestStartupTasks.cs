using Biwen.QuickApi.Infrastructure.StartupTask;

namespace Biwen.QuickApi.DemoWeb.TestStartupTasks
{
    public class TestStartup(ILogger<TestStartup> logger) : IStartupTask
    {
        public int Order => 1;

        public TimeSpan? Delay => null;

        public Task ExecuteAsync(CancellationToken ct)
        {
            logger.LogInformation("TestStartup !!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            return Task.CompletedTask;
        }
    }


    public class TestStartup2(ILogger<TestStartup2> logger) : IStartupTask
    {
        public int Order => 1;

        public TimeSpan? Delay => TimeSpan.FromSeconds(10d);

        public Task ExecuteAsync(CancellationToken ct)
        {
            logger.LogInformation("TestStartup2 !!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            return Task.CompletedTask;
        }
    }


    public class TestStartup3 : StartupTaskBase
    {
        public override TimeSpan? Delay => TimeSpan.FromSeconds(10d);

        public override Task ExecuteAsync(CancellationToken ct)
        {
            throw new Exception("TestStartup3 Error");
        }
    }
}