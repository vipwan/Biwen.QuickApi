using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;

namespace BenchmarkTestWeb.QuickApis
{

    [QuickApi("my-quickapi", Verbs = Verb.POST)]
    [OpenApiMetadata("quickapi", "quickapi")]
    //[OpenApiTags("API")]
    //[Tags("API")]
    public class MyApi : BaseQuickApi<MyRequest, IResult>
    {
        public override async ValueTask<IResult> ExecuteAsync(MyRequest request)
        {
            await Task.CompletedTask;
            return Results.Ok(request);
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithTags("API");
            return base.HandlerBuilder(builder);
        }
    }
}