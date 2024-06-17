namespace Biwen.QuickApi.DemoWeb.Apis
{

    [QuickApi("middleware")]
    public class MiddlewareApi : BaseQuickApi
    {

        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Results.Ok();
        }


        public override Task BeforeAsync(HttpContext context, CancellationToken cancellationToken)
        {
            return base.BeforeAsync(context, cancellationToken);
        }

        public override Task AfterAsync(HttpContext context, CancellationToken cancellationToken)
        {
            context.Response.ContentType = "text/plain";
            context.Response.WriteAsync($"middleware:{DateTime.Now}", default);
            return Task.CompletedTask;
        }
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //5d过期
            builder.CacheOutput(x => x.Expire(TimeSpan.FromSeconds(5d)));
            //builder.WithTags("custom");
            return base.HandlerBuilder(builder);
        }
    }
}