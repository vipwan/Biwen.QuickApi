using Biwen.QuickApi.Attributes;

namespace Biwen.QuickApi.SourceGenerator.TestConsole
{




    /// <summary>
    /// 模拟一个空的请求
    /// </summary>
    [QuickApi("test1")]
    public class TestQuickApi : BaseQuickApi
    {
        public override async Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return EmptyResponse.New;
        }
    }

    /// <summary>
    /// 模拟一个POST空的请求
    /// </summary>
    [QuickApi("test2", Verbs = Verb.POST)]
    public class TestPostQuickApi : BaseQuickApi
    {
        public override async Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return EmptyResponse.New;
        }
    }
}