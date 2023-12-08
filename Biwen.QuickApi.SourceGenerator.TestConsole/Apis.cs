namespace Biwen.QuickApi.SourceGenerator.TestConsole
{
    using Biwen.QuickApi.Attributes;
    using FluentValidation;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;

    public class HelloRequest : BaseRequest<HelloRequest>
    {
        public string Name { get; set; } = null!;

        public HelloRequest()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is empty");
        }
    }

    public class HelloResponse : BaseResponse
    {
        public HelloResponse(string hello, string world)
        {
            Hello = hello;
            World = world;
        }

        public string Hello { get; set; } = null!;
        public string World { get; set; } = null!;

    }





    /// <summary>
    /// 模拟一个空的请求
    /// </summary>
    [QuickApi("test1", Group = "hello", Policy = "admin", Verbs = Verb.GET | Verb.POST)]
    [Authorize("admin")]
    public class TestQuickApi : BaseQuickApi
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return Results.Ok().AsRspOfResult();
        }
    }

    /// <summary>
    /// 模拟一个POST空的请求
    /// </summary>
    [QuickApi("test2", Verbs = Verb.POST)]
    public class TestPostQuickApi : BaseQuickApi
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return Results.Ok().AsRspOfResult();
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {

            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "This is a summary 22222222",
                Description = "This is a description"
            });

            return base.HandlerBuilder(builder);
        }

    }


    /// <summary>
    /// 模拟一个自定义的请求
    /// </summary>
    [QuickApi("test3", Verbs = Verb.GET | Verb.POST), JustAsService]
    public class Test3PostQuickApi : BaseQuickApi<HelloRequest, ContentResponse>
    {
        public override async ValueTask<ContentResponse> ExecuteAsync(HelloRequest request)
        {
            await Task.CompletedTask;
            return new ContentResponse($"hello {request.Name}");
        }
    }

    [QuickApi("hello4")]
    public class Tset3QuickApi : BaseQuickApi<HelloRequest, HelloResponse>
    {

        public override async ValueTask<HelloResponse> ExecuteAsync(HelloRequest request)
        {
            await Task.CompletedTask;
            return new HelloResponse("hello", request.Name);
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "This is a summary",
                Description = "This is a description"
            });

            return base.HandlerBuilder(builder);
        }
    }

    [QuickApi("test6")]
    public class Test6 : BaseQuickApi
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return Results.Ok().AsRspOfResult();
        }
    }
}