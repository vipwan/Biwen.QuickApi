Blazor模板引擎
=====================

用于需要在服务端生成HTML的场景,例如邮件模板,短信模板等


开发用例
---------------------

在Components文件夹中添加Blazor组件 `RenderMessage.razor`

```csharp
@namespace Biwen.QuickApi.Test.Components
<h1>Render Message</h1>
<p>@Message</p>
@code {
    [Parameter]
    public string? Message { get; set; }
}
```


在QuickEndpoint中使用:

```csharp
[EndpointGroupName("test")]
[ProducesResponseType<string>(200)]
[OpenApiMetadata("BlazorSvc渲染测试", "Blazor渲染测试", Tags = ["Endpoints"])]
public class BlazorRenderSvcEndpoint : IQuickEndpoint
{
    public static Delegate Handler => async (BlazorRendererService rendererService) =>
    {
        var dictionary = new Dictionary<string, object?>
            {
                { "Message", "Hello from the Render Message component!" }
            };

        var html = await rendererService.Render<RenderMessage>(dictionary);
        return Results.Content(html, "text/html");
    };

    public static Verb Verbs => Verb.GET;
}
```
```csharp
app.MapMethods<BlazorRenderSvcEndpoint>("hello/blazor-render-svc");
```

测试用例

```csharp
using Biwen.QuickApi.Test.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.Test
{
    public class BlazorRendererServiceTest(ITestOutputHelper testOutput)
    {
        //测试HtmlRenderer:
        [Fact]
        public async Task ShouldRenderHtml()
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
                { "Message", "Hello from the Render Message component!" }
            };
            var html = await service.Render<RenderMessage>(pairs);

            testOutput.WriteLine(html);
            Assert.Contains("Hello from the Render Message component!", html);

        }
    }
}
```

注意事项
---------------------

模板引擎仅仅是渲染组件到HTML,不支持组件的交互行为,如Blazor按钮的事件绑定,因为组件是在服务端渲 没有Blazor的生命周期!<br/>
`OnInitialized`被视为构造函数是允许的,你也可以在视图组件中注入服务.比如IttpContextAccessor,ILogger等<br/>
你可以简单的认为模板引擎可以在初始化中执行任何事情,但是回调和组件内部异步行为不被允许!



API文档
---------------------

相关API文档:

[BlazorRendererService](../api/Biwen.QuickApi.Http.BlazorRendererService.yml)
[Components](https://learn.microsoft.com/zh-cn/aspnet/core/blazor/components/?view=aspnetcore-9.0)