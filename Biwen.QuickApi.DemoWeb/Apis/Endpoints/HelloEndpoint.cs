using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Biwen.QuickApi.DemoWeb.Apis.Endpoints
{
    /// <summary>
    /// 测试IQuickEndpoint
    /// </summary>
    [Authorize]
    [Produces<string>]
    public class HelloEndpoint : IQuickEndpoint
    {
        /// <summary>
        /// 参数绑定可以实现IReqBinder<T>自定义绑定,也可以使用AsParameters在MinimalApi中默认的绑定
        /// </summary>
        public class HelloRequest : BaseRequest<HelloRequest>//, IReqBinder<HelloRequest>
        {
            [FromRoute]
            public string? Hello { get; set; }

            [FromQuery]
            public string? Key { get; set; }

        }


        public static Delegate Handler => ([AsParameters] HelloRequest request) =>
        {
            //直接返回Content
            return Results.Content($"{request.Hello}. {request.Key}");
        };

        public static Verb Verbs => Verb.GET;
    }
}