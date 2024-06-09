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
            [Description("Hello的描述")]
            [FromRoute]
            public string? Hello { get; set; }

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

}