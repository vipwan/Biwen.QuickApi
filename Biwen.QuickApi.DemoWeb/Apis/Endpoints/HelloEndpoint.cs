using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Biwen.QuickApi.DemoWeb.Apis.Endpoints
{

    /// <summary>
    /// 测试IQuickEndpoint
    /// </summary>
    public class HelloEndpoint : IQuickEndpoint
    {
        public class HelloRequest : BaseRequest<HelloRequest>, IReqBinder<HelloRequest>
        {
            [FromRoute]
            public string? Hello { get; set; }

            [FromQuery]
            public string? Key { get; set; }

            public static async ValueTask<HelloRequest?> BindAsync(HttpContext context, ParameterInfo parameter = null!)
            {
                var req = await DefaultReqBinder<HelloRequest>.BindAsync(context, parameter);
                return req;
            }
        }

        public static Delegate Handler()
        {
            var invoke = (HelloRequest request) =>
            {
                //直接返回Content
                return Results.Content($"{request.Hello}. {request.Key}");
            };
            return invoke;
        }

        public static Verb Verbs => Verb.GET;
    }
}