using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Biwen.QuickApi.DemoWeb.Apis
{
    public class RouteData : BaseRequest<RouteData>
    {
        [Description("hello的描述")]
        public string? Hello { get; set; }

        [Description("world的描述")]
        [MaxLength(5)]
        public string? World { get; set; }
    }

    public class Route2Data : BaseRequest<Route2Data>
    {
        [Description("hello的描述")]
        [FromRoute]
        public string? Hello { get; set; }

        [FromQuery]
        [Description("world的描述")]
        public string? World { get; set; }

        [Description("post data")]
        public string? PostData { get; set; }

    }



    [QuickApi("{hello}/{world}", Group = "route")]
    [OpenApiMetadata("RoutesApi", "RoutesApi", Tags = ["QuickApis"])]
    public class RoutesApi : BaseQuickApi<RouteData>
    {
        public override async ValueTask<IResult> ExecuteAsync(RouteData request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Results.Content($"{request.Hello} {request.World}");
        }
    }

    [QuickApi("route2-{hello}", Group = "route", Verbs = Verb.POST)]
    [OpenApiMetadata("Route2Api", "Route2Api", Tags = ["QuickApis"])]
    public class Route2Api(HelloService helloService) : BaseQuickApi<Route2Data, Route2Data>
    {
        public override async ValueTask<Route2Data> ExecuteAsync(Route2Data request, CancellationToken cancellationToken)
        {
            var hello = helloService.Hello("78");
            await Task.CompletedTask;
            return request;
        }
    }

}