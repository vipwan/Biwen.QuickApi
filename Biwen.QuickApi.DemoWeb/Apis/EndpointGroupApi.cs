﻿namespace Biwen.QuickApi.DemoWeb.Apis
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
}