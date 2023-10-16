﻿

namespace Biwen.QuickApi.DemoWeb.Apis
{

    [QuickApi("endpointgroup")]
    [QuickApiSummary("分组测试", "分组测试")]
    [EndpointGroupName("group1")]
    public class EndpointGroupApi : BaseQuickApi
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(EmptyResponse.New);
        }


        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithTags("group");

            //builder.WithGroupName("group1");
            return base.HandlerBuilder(builder);
        }
    }
}