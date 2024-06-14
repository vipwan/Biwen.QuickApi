namespace Biwen.QuickApi.DemoWeb.Apis
{

    [QuickApi("middleware")]
    public class MiddlewareApi : BaseQuickApi
    {

        public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return Results.Ok();
        }


        public override Task BeforeAsync(HttpContext context)
        {
            return base.BeforeAsync(context);
        }

        public override Task AfterAsync(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.WriteAsync("middleware");
            return Task.CompletedTask;
        }
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithTags("custom");
            return base.HandlerBuilder(builder);
        }
    }
}