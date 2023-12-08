
using Biwen.QuickApi.Attributes;

namespace Biwen.QuickApi.SourceGenerator.TestConsole.Api2
{


    [QuickApi("test1", Group = "api2", Verbs = Verb.GET)]
    public class TestQuickApi : BaseQuickApi
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return Results.Ok().AsRspOfResult();
        }
    }


    [QuickApi("iresult", Verbs = Verb.GET)]
    public class TestIResultQuickApi : BaseQuickApiWithoutRequest<IResultResponse>
    {
        public override ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            return new ValueTask<IResultResponse>(new IResultResponse(Results.Json(new { Hello = "world" })));
        }
    }


    /// <summary>
    /// JustAsService
    /// </summary>
    [QuickApi(""), JustAsService]
    public class TJustAsServiceQuickApi : BaseQuickApiWithoutRequest<IResultResponse>
    {
        public override ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            return new ValueTask<IResultResponse>(new IResultResponse(Results.Json(new { Hello = "world" })));
        }
    }

}