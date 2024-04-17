using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

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

        public override async ValueTask<IResultResponse> ExecuteAsync(HelloApiRequest request)
        {
            await Task.CompletedTask;
            Console.WriteLine($"获取自定义的 CustomApi:,从querystring:c绑定,{request.Name}");
            return IResultResponse.OK();
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
                context.HttpContext.Response.Headers.TryAdd("X-Powered-By", "Biwen.QuickApi");
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
    [QuickApiSummary("已过时的接口", "DeprecatedApi", IsDeprecated = true)]
    public class DeprecatedApi : BaseQuickApi
    {

        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return IResultResponse.OK();
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


    [QuickApi("throw", Verbs = Verb.GET)]
    [QuickApiSummary("抛出异常的接口", "抛出异常,测试500错误格式化")]
    public class ThrowErrorApi : BaseQuickApi
    {
        public override ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            throw new NotImplementedException();
        }
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithTags("custom");
            return base.HandlerBuilder(builder);
        }
    }


    [QuickApi("cache", Verbs = Verb.GET | Verb.POST)]
    [QuickApiSummary("Cache缓存测试", "测试缓存功能")]
    [OutputCache(Duration = 10)]
    public class CachedApi : BaseQuickApiWithoutRequest<IResultResponse>
    {
        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return IResultResponse.Content(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {

            builder.CacheOutput(x =>
            {
                x.Expire(TimeSpan.FromSeconds(5));
            });

            builder.WithTags("custom");
            return base.HandlerBuilder(builder);
        }
    }



    [QuickApi("ant-ui")]
    public class AntUI : BaseQuickApiWithoutRequest<IResultResponse>
    {
        private readonly IAntiforgery _antiforgery;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AntUI(IAntiforgery antiforgery, IHttpContextAccessor httpContextAccessor)
        {
            _antiforgery = antiforgery;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            var token = _antiforgery.GetAndStoreTokens(_httpContextAccessor.HttpContext!);
            var html = $"""
              <html>
                <body>
                 <h3>Upload a image test</h3>
                  <form name="form1" action="/quick/ant" method="post" enctype="multipart/form-data">
                    <input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}"/>
                    <input type="file" name="file" placeholder="Upload an image..." accept=".jpg,.png" />
                    <input type="submit" />
                  </form> 
                </body>
              </html>
            """;
            await Task.CompletedTask;
            return Results.Content(html, "text/html").AsRspOfResult();
        }
    }


    public class AntRequest : BaseRequest<AntRequest>
    {
        /// <summary>
        /// 上传的文件
        /// </summary>
        public IFormFile? File { get; set; }

        public AntRequest()
        {
            RuleFor(x => x.File).NotNull();
        }
    }

    [QuickApi("ant", Verbs = Verb.POST)]
    [QuickApiSummary("请注意不能在swaggerUI中测试", "请注意不能在swaggerUI中测试,因为没有给定正确的RequestToken,请打开quick/ant-ui测试通过")]
    public class AntApi : BaseQuickApi<AntRequest, IResultResponse>
    {
        /// <summary>
        /// 启动防伪验证
        /// </summary>
        public override bool IsAntiforgeryEnabled => true;

        public override async ValueTask<IResultResponse> ExecuteAsync(AntRequest request)
        {
            await Task.CompletedTask;
            //return "Successed!".AsRspOfResult();
            return Results.File(request.File!.OpenReadStream(), "image/png").AsRspOfResult();
        }

        //public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        //{
        //    //上传文件必须使用 multipart/form-data
        //    builder.Accepts<AntRequest>("multipart/form-data");

        //    return base.HandlerBuilder(builder);
        //}
    }


}