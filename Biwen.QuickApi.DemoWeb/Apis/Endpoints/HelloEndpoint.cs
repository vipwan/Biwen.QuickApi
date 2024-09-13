using Biwen.QuickApi.DemoWeb.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FeatureManagement.Mvc;
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
    [AuditApi]//审计
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
    [OpenApiMetadata("BlazorSvc渲染测试", "Blazor渲染测试", Tags = ["Endpoints"])]
    public class BlazorRenderSvcEndpoint : IQuickEndpoint
    {
        public static Delegate Handler => async (BlazorRendererService rendererService) =>
        {
            var dictionary = new Dictionary<string, object?>
                {
                    { "Message", "Hello from the Render Message component!" }
                };

            var html = await rendererService.RenderAsync<RenderMessage>(dictionary);
            return Results.Content(html, "text/html");
        };

        public static Verb Verbs => Verb.GET;
    }



    [EndpointGroupName("test")]
    [ProducesResponseType<string>(200)]
    [OpenApiMetadata("Feature测试", "Feature测试", Tags = ["Endpoints"])]
    [FeatureGate("myfeature")]
    public class FeatureTestEndpoint : IQuickEndpoint
    {
        public static Delegate Handler => () =>
        {
            return Results.Content("new feature!");
        };

        public static Verb Verbs => Verb.GET;
    }





}