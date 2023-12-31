﻿

namespace Biwen.QuickApi.DemoWeb.Apis
{

    [QuickApi("endpointgroup")]
    [QuickApiSummary("分组测试", "分组测试")]
    [EndpointGroupName("group1")]
    public class EndpointGroupApi : BaseQuickApi
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return Results.Ok().AsRspOfResult();
        }


        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithTags("group");

            //builder.WithGroupName("group1");
            return base.HandlerBuilder(builder);
        }
    }
}