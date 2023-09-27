
namespace Biwen.QuickApi.SourceGenerator.TestConsole
{
    using Biwen.QuickApi.Attributes;
    using FluentValidation;
    using Microsoft.AspNetCore.Authorization;

    public class HelloRequest : BaseRequest<HelloRequest>
    {
        public string Name { get; set; } = null!;

        public HelloRequest()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is empty");
        }
    }


    /// <summary>
    /// 模拟一个空的请求
    /// </summary>
    [QuickApi("test1",Group ="hello",Policy ="admin",Verbs = Verb.GET| Verb.POST)]
    [Authorize("admin")]
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


    /// <summary>
    /// 模拟一个自定义的请求
    /// </summary>
    [QuickApi("test3", Verbs = Verb.GET | Verb.POST), JustAsService]
    public class Test3PostQuickApi : BaseQuickApi<HelloRequest, ContentResponse>
    {
        public override async Task<ContentResponse> ExecuteAsync(HelloRequest request)
        {
            await Task.CompletedTask;
            return new ContentResponse($"hello {request.Name}");
        }
    }
}