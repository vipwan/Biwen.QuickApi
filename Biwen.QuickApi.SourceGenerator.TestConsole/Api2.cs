
using Biwen.QuickApi.Attributes;

namespace Biwen.QuickApi.SourceGenerator.TestConsole.Api2
{


    [QuickApi("test1", Group = "api2", Verbs = Verb.GET)]
    public class TestQuickApi : BaseQuickApi
    {
        public override async Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return EmptyResponse.New;
        }
    }


}
