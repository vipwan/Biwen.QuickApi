namespace Biwen.QuickApi.Test
{

    public class DecorateTest(ITestOutputHelper testOutput)
    {
        [Fact]
        public async Task TestDecorate()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITestService, TestService>();
            services.Decorate<ITestService, DecorateTestService>();
            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<ITestService>();
            var result = await service.SayHello();

            testOutput.WriteLine(result);

            Assert.Equal("Decorated: hello world", result);
        }


        interface ITestService
        {
            Task<string?> SayHello();
        }

        class TestService : ITestService
        {
            public async Task<string?> SayHello()
            {
                await Task.CompletedTask;
                return "hello world";
            }
        }

        class DecorateTestService(ITestService inner) : ITestService
        {
            public async Task<string?> SayHello()
            {
                var old = await inner.SayHello();
                return $"Decorated: {old}";
            }
        }
    }
}