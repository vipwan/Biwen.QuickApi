
using Biwen.QuickApi.Attributes;

namespace Biwen.QuickApi.SourceGenerator.TestConsole.Api2
{


    [QuickApi("test1", Group = "api2", Verbs = Verb.GET)]
    public class TestQuickApi : BaseQuickApi
    {
        public override async Task<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return Results.Ok().AsRspOfResult();
        }
    }


    [QuickApi("iresult", Verbs = Verb.GET)]
    public class TestIResultQuickApi : BaseQuickApiWithoutRequest<IResultResponse>
    {
        public override Task<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(new IResultResponse(Results.Json(new { Hello = "world" })));
        }
    }
}