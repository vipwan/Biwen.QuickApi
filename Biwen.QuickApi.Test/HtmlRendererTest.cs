using Alba;
using Biwen.QuickApi.Test.Components;

namespace Biwen.QuickApi.Test
{
    public class BlazorRendererServiceTest : IClassFixture<TestBase.QuickApiTestFactory>
    {

        private readonly ITestOutputHelper _testOutput;
        private readonly TestBase.QuickApiTestFactory _factory;

        public BlazorRendererServiceTest(TestBase.QuickApiTestFactory factory, ITestOutputHelper testOutput)
        {
            _factory = factory;
            _testOutput = testOutput;
        }


        //测试HtmlRenderer:
        [Theory]
        [InlineData("Hello from the Render Message component!")]
        [InlineData("hello 2")]
        [InlineData("hello 3")]
        public async Task ShouldRenderHtml(string message)
        {
            //var env = WebApplication.CreateSlimBuilder().Environment;

            //IServiceCollection services = new ServiceCollection();
            //services.AddLogging();

            ////添加Env
            //services.AddSingleton(env);

            ////添加Configuration
            //var configuration = new ConfigurationBuilder().Build();
            //services.AddSingleton<IConfiguration>(configuration);

            //services.AddHttpContextAccessor();
            ////提供Blazor组件服务
            //services.AddRazorComponents().AddInteractiveServerComponents();

            ////提供Blazor组件渲染服务
            //services.AddScoped<BlazorRendererService>();

            //var serviceProvider = services.BuildServiceProvider();

            //var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            //httpContextAccessor.HttpContext = new DefaultHttpContext();

            //var service = serviceProvider.GetRequiredService<BlazorRendererService>();

            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<BlazorRendererService>();

            var pairs = new Dictionary<string, object?>
            {
                { "Message", message }
            };
            var html = await service.Render<RenderMessage>(pairs);

            _testOutput.WriteLine(html);
            Assert.Contains(message, html);

        }

        //使用alba测试 HtmlRenderer:
        [Theory]
        [InlineData("Hello from the Render Message component!")]
        [InlineData("hello 2")]
        [InlineData("hello 3")]
        public async Task ShouldRenderHtml_With_Alba(string message)
        {
            var host = await AlbaHost.For<DemoWeb.Program>();

            using var scope = host.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<BlazorRendererService>();
            var pairs = new Dictionary<string, object?>
            {
                { "Message", message }
            };
            var html = await service.Render<RenderMessage>(pairs);

            _testOutput.WriteLine(html);
            Assert.Contains(message, html);
        }
    }
}