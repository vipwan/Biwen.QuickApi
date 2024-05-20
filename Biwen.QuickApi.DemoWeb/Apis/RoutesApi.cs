using System.ComponentModel;

namespace Biwen.QuickApi.DemoWeb.Apis
{
    public class RouteData : BaseRequest<RouteData>
    {
        [Description("hello的描述")]
        public string? Hello { get; set; }

        [Description("world的描述")]
        public string? World { get; set; }
    }

    [QuickApi("{hello}/{world}", Group = "route")]
    public class RoutesApi : BaseQuickApi<RouteData>
    {
        public override async ValueTask<IResult> ExecuteAsync(RouteData request)
        {
            await Task.CompletedTask;
            return Results.Content($"{request.Hello} {request.World}");
        }
    }
}