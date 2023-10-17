using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using NSwag.Annotations;

namespace BenchmarkTestWeb.QuickApis
{

    [QuickApi("my-quickapi", Verbs = Verb.POST)]
    [QuickApiSummary("quickapi", "quickapi")]
    //[OpenApiTags("API")]
    //[Tags("API")]
    public class MyApi : BaseQuickApi<MyRequest, IResultResponse>
    {
        public override async Task<IResultResponse> ExecuteAsync(MyRequest request)
        {
            return await Task.FromResult(Results.Ok(request).AsRspOfResult());
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithTags("API");
            return base.HandlerBuilder(builder);
        }
    }
}