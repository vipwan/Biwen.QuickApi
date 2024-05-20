namespace Biwen.QuickApi.DemoWeb.Apis
{
    public class RouteData : BaseRequest<RouteData>
    {
        public string? Hello { get; set; }

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