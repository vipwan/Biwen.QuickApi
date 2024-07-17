using Biwen.QuickApi.Test.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.Test
{
    public class BlazorRendererServiceTest(ITestOutputHelper testOutput)
    {
        //测试HtmlRenderer:
        [Theory]
        [InlineData("Hello from the Render Message component!")]
        [InlineAutoData]
        [InlineAutoData]
        public async Task ShouldRenderHtml(string message)
        {
            var env = WebApplication.CreateSlimBuilder().Environment;

            IServiceCollection services = new ServiceCollection();
            services.AddLogging();

            //添加Env
            services.AddSingleton(env);

            //添加Configuration
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);

            services.AddHttpContextAccessor();
            //提供Blazor组件服务
            services.AddRazorComponents().AddInteractiveServerComponents();

            //提供Blazor组件渲染服务
            services.AddScoped<BlazorRendererService>();

            var serviceProvider = services.BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            httpContextAccessor.HttpContext = new DefaultHttpContext();

            var service = serviceProvider.GetRequiredService<BlazorRendererService>();

            var pairs = new Dictionary<string, object?>
            {
                { "Message", message }
            };
            var html = await service.Render<RenderMessage>(pairs);

            testOutput.WriteLine(html);
            Assert.Contains(message, html);

        }
    }
}