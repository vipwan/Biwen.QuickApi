namespace Biwen.QuickApi.DemoWeb.Apis
{
    /// <summary>
    /// get ~/custom?c=11112&p=12345&u=1234567
    /// </summary>
    [QuickApi("custom", Verbs = Verb.GET)]
    [QuickApiSummary("自定义绑定", "自定义绑定.系统生成的SwagDoc传参没有意义,请按照实际情况传参,\r\n 请求路径: ~/custom?c=11112&p=12345&u=1234567")]
    public class CustomApi : BaseQuickApi<HelloApiRequest>
    {
        public CustomApi()
        {
            UseReqBinder<CustomApiRequestBinder>();
        }

        public override async Task<EmptyResponse> ExecuteAsync(HelloApiRequest request)
        {
            await Task.CompletedTask;
            Console.WriteLine($"获取自定义的 CustomApi:,从querystring:c绑定,{request.Name}");
            return EmptyResponse.New;
        }

        /// <summary>
        /// 提供minimal扩展
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //自定义标签
            builder.WithTags("custom");

            //自定义过滤器
            builder.AddEndpointFilter(async (context, next) =>
            {
                Console.WriteLine("自定义过滤器!");
                return await next(context);
            });

            //NSwag 必须使用 OpenApiOperationAttribute 或者 推荐使用的:QuickApiSummaryAttribute
            //Swashbuckle 使用 WithOpenApi
            //builder.WithOpenApi(operation => new(operation)
            //{
            //    Summary = "custom",
            //    Description = "自定义绑定.系统生成的SwagDoc传参没有意义,请按照实际情况传参"
            //});
            return base.HandlerBuilder(builder);
        }
    }


    [QuickApi("deprecated", Verbs = Verb.GET)]
    [QuickApiSummary("已过时的接口", "DeprecatedApi",IsDeprecated = true)]
    public class DeprecatedApi : BaseQuickApi
    {

        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            return base.ExecuteAsync(request);
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithTags("custom");

            // NSwag不被支持,请使用QuickApiSummaryAttribute
            //builder.WithOpenApi(operation => new(operation)
            //{
            //    Deprecated = true
            //});

            return base.HandlerBuilder(builder);
        }

    }

}