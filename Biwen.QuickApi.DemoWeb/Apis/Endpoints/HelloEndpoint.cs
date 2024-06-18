using Biwen.QuickApi.DemoWeb.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Biwen.QuickApi.DemoWeb.Apis.Endpoints
{
    /// <summary>
    /// 测试IQuickEndpoint
    /// </summary>
    //[Authorize]
    [ProducesResponseType<ProblemDetails>(400)]
    [ProducesResponseType<string>(200)]
    [EndpointGroupName("test")]
    [OpenApiMetadata("IQuickEndpoint测试", "测试IQuickEndpoint", Tags = ["Endpoints"], IsDeprecated = true, OperationId = "TestHelloEndpoint")]
    public class HelloEndpoint : IQuickEndpoint
    {
        /// <summary>
        /// 参数绑定可以实现IReqBinder<T>自定义绑定,也可以使用AsParameters在MinimalApi中默认的绑定
        /// </summary>
        public class HelloRequest : BaseRequest<HelloRequest>//, IReqBinder<HelloRequest>
        {
            [Required, Length(5, 12)]
            [Description("Hello的描述")]
            [FromRoute]
            public string Hello { get; set; } = null!;

            [Description("Key的描述")]
            [Length(5, 12)]
            [FromQuery]
            public string? Key { get; set; }

        }

        public static Delegate Handler => ([AsParameters] HelloRequest request) =>
        {
            //直接返回Content
            return Results.Content($"{request.Hello}. {request.Key}");
        };

        public static Verb Verbs => Verb.POST;
    }

    /// <summary>
    /// 测试IQuickEndpoint
    /// </summary>
    //[Authorize]
    [EndpointGroupName("test")]
    [ProducesResponseType<string>(200)]
    [OpenApiMetadata("Blazor渲染测试", "Blazor渲染测试", Tags = ["Endpoints"])]
    public class BlazorRenderEndpoint : IQuickEndpoint
    {
        public static Delegate Handler => async (IServiceProvider serviceProvider) =>
        {
            await using var htmlRenderer = new HtmlRenderer(
                serviceProvider,
            serviceProvider.GetRequiredService<ILoggerFactory>());

            var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var dictionary = new Dictionary<string, object?>
                {
                    { "Message", "Hello from the Render Message component!" }
                };

                var parameters = ParameterView.FromDictionary(dictionary);
                var output = await htmlRenderer.RenderComponentAsync<RenderMessage>(parameters);

                return output.ToHtmlString();
            });

            return Results.Content(html, "text/html");


        };

        public static Verb Verbs => Verb.GET;
    }

    /// <summary>
    /// 测试IQuickEndpoint
    /// </summary>
    //[Authorize]
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


}