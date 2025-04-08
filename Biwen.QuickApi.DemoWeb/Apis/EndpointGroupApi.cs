namespace Biwen.QuickApi.DemoWeb.Apis
{
    [QuickApi("endpointgroup")]
    [OpenApiMetadata("分组测试", "分组测试")]
    [EndpointGroupName("group1")]
    public class EndpointGroupApi : BaseQuickApi
    {
        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Results.Ok();
        }


        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithTags("group");

            //builder.WithGroupName("group1");
            return base.HandlerBuilder(builder);
        }
    }

    [QuickApi("ctx")]
    [OpenApiMetadata("ctx测试", "ctx测试")]
    public class CtxApi : BaseQuickApi
    {
        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var ctx = HttpContext;

            //使用Httpcontext
            ctx.Response.StatusCode = 403;

            return Results.Content("ctx");

        }
    }

}