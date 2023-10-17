namespace Biwen.QuickApi.SourceGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    //using System.Threading;
    using Microsoft.CodeAnalysis;
    //using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [Generator(LanguageNames.CSharp)]
#pragma warning disable RS1036 // 指定分析器禁止的 API 强制设置
    public class QuickApiSourceGenerator : IIncrementalGenerator
#pragma warning restore RS1036 // 指定分析器禁止的 API 强制设置
    {

        #region template

        const string endpointTemp = $@"
//code gen for Biwen.QuickApi
//version :$version
//author :vipwan@outlook.com 万雅虎
//https://github.com/vipwan/Biwen.QuickApi
//如果你在使用中遇到问题,请第一时间issue,谢谢!

using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using Biwen.QuickApi.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

$namespace

namespace Biwen.QuickApi;

#pragma warning disable
public static partial class AppExtentions
{{

    /// <summary>
    /// 注册所有的QuickApi(Gen版)
    /// </summary>
    /// <param name=""app""></param>
    /// <param name=""serviceProvider""></param>
    /// <param name=""prefix"">全局路由前缀</param>
    /// <returns></returns>
    /// <exception cref=""ArgumentNullException""></exception>
    /// <exception cref=""QuickApiExcetion""></exception>
    public static RouteGroupBuilder MapGenQuickApis(this IEndpointRouteBuilder app, IServiceProvider serviceProvider,string prefix = ""api"")
    {{
        if (string.IsNullOrEmpty(prefix))
        {{
            throw new ArgumentNullException(nameof(prefix));
        }}
        //middleware:
        (app as WebApplication)?.UseMiddleware<QuickApiMiddleware>();
        var groupBuilder = app.MapGroup(prefix);
        using var scope = serviceProvider.CreateScope();
        $apis
        return groupBuilder;
    }}

    /// <summary>
    /// 验证Policy
    /// </summary>
    /// <exception cref=""QuickApiExcetion""></exception>
    async static Task<(bool Flag, IResult? Result)> CheckPolicy(IHttpContextAccessor ctx, string? policy)
    {{
        if (string.IsNullOrEmpty(policy))
        {{
            return (true, null);
        }}
        if (!string.IsNullOrEmpty(policy))
        {{
            var httpContext = ctx.HttpContext;
            var authService = httpContext!.RequestServices.GetService<IAuthorizationService>() ?? throw new QuickApiExcetion($""IAuthorizationService is null, besure services.AddAuthorization() first!"");
            var authorizationResult = await authService.AuthorizeAsync(httpContext.User, policy);
            if (!authorizationResult.Succeeded)
            {{
                return (false, TypedResults.Unauthorized());
            }}
        }}
        return (true, null);
    }}

    /// <summary>
    /// 内部返回的Result
    /// </summary>
    static (bool Flag, IResult? Result) InnerResult(object? result)
    {{
        if (result is EmptyResponse)
        {{
            return (true, TypedResults.Ok());
        }}
        if (result is ContentResponse content)
        {{
            return (true, TypedResults.Content(content.ToString()));
        }}
        if (result is IResultResponse iresult)
        {{
            return (true, iresult.Result);
        }}
        return (false, null);
    }}
}}
#pragma warning restore
";

        const string routeTemp = $@"
        var $4 = groupBuilder.MapMethods(""$0"", new[] {{ $1 }}, async (IHttpContextAccessor ctx, $3 api) =>
            {{
                var checkResult = await CheckPolicy(ctx, ""$2"");
                if (!checkResult.Flag) return checkResult.Result!;
                var req = await api.ReqBinder.BindAsync(ctx.HttpContext!);
                var vresult = req.Validate();
                if (!vresult.IsValid) {{ return TypedResults.ValidationProblem(vresult.ToDictionary()); }}
                try
                {{
                    var result = await api.ExecuteAsync(req!);
                    var resultFlag = InnerResult(result);
                    if (resultFlag.Flag) return resultFlag.Result!;
                    return TypedResults.Json(result);
                }}
                catch (Exception ex)
                {{
                    var exceptionHandlers = ctx.HttpContext!.RequestServices.GetServices<IQuickApiExceptionHandler>();
                    foreach (var handler in exceptionHandlers)
                    {{
                        await handler.HandleAsync(ex);
                    }}
                    var exceptionResultBuilder = ctx.HttpContext!.RequestServices.GetRequiredService<IQuickApiExceptionResultBuilder>();
                    return await exceptionResultBuilder.ErrorResult(ex);
                }}
            }});
        //metadata
        $4.WithMetadata(new QuickApiMetadata(typeof($3)));
        //handlerbuilder
        scope.ServiceProvider.GetRequiredService<$3>().HandlerBuilder($4);
        //outputcache
        var $4Cache = typeof($3).GetCustomAttribute<OutputCacheAttribute>();
        if ($4Cache != null) $4.WithMetadata($4Cache);
        //endpointgroup
        var $4EndpointgroupAttribute = typeof($3).GetCustomAttribute<EndpointGroupNameAttribute>();
        if ($4EndpointgroupAttribute != null) $4.WithMetadata($4EndpointgroupAttribute);
        //authorizeattribute
        var $4authorizeAttributes = typeof($3).GetCustomAttributes<AuthorizeAttribute>();
        if ($4authorizeAttributes.Any()) $4.WithMetadata(new AuthorizeAttribute());
        foreach (var authAttr in $4authorizeAttributes)
        {{
            $4.WithMetadata(authAttr);
        }}
        //allowanonymous
        var $4allowanonymous = typeof($3).GetCustomAttribute<AllowAnonymousAttribute>();
        if ($4allowanonymous != null) $4.WithMetadata($4allowanonymous);
";

        #endregion

        #region helper

        /// <summary>
        /// 将字符串转换为Verbs
        /// </summary>
        /// <param name="verbs"></param>
        /// <returns></returns>
        static List<string> ToVerbs(string? verbs)
        {

            if (string.IsNullOrEmpty(verbs)) { return new List<string>() { "GET" }; }
            var list = new List<string>();
            if (verbs!.Contains("GET"))
            {
                list.Add("GET");
            }
            if (verbs.Contains("POST"))
            {
                list.Add("POST");
            }
            if (verbs.Contains("PUT"))
            {
                list.Add("PUT");
            }
            if (verbs.Contains("DELETE"))
            {
                list.Add("DELETE");
            }
            if (verbs.Contains("PATCH"))
            {
                list.Add("PATCH");
            }
            if (verbs.Contains("HEAD"))
            {
                list.Add("HEAD");
            }
            if (verbs.Contains("OPTIONS"))
            {
                list.Add("OPTIONS");
            }

            return list;
        }

        #endregion

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            // quickApi
            var quickApiProvider = context.SyntaxProvider.CreateSyntaxProvider(
                (syntax, cancellationToken) => syntax is ClassDeclarationSyntax
                {
                    BaseList: not null
                },
                (context, cancellationToken) =>
                (ClassDeclarationSyntax)context.Node)
                     .Where(x => x.AttributeLists.Count > 0)
                     .Where(x => x.AttributeLists.SelectMany(x => x.Attributes)
                     .Any(x => x.Name.ToString() == QuickApiType.TypeName)).Collect();



            //顶级语句是没有main函数的
            //var main = context.SyntaxProvider.CreateSyntaxProvider(
            //                   (syntax, cancellationToken) => syntax is MethodDeclarationSyntax
            //                   {
            //                       Identifier: { ValueText: "Main" }
            //                   },
            //                   (context, cancellationToken) =>
            //                   (MethodDeclarationSyntax)context.Node).Collect();

            //context.RegisterSourceOutput(main, (ctx, source) =>
            //{
            //    var method = source.AsEnumerable();
            //    var len = method.Count();

            //});


            context.RegisterSourceOutput(quickApiProvider, (ctx, source) =>
            {
                #region 生成QuickApi代码

                var sb = new StringBuilder();

                //存储所有的命名空间
                IList<string> namespaces = new List<string>();

                foreach (var classDeclarationSyntax in source.AsEnumerable())
                {
                    var fullname = classDeclarationSyntax.Identifier.ValueText;
                    var attrs = classDeclarationSyntax.AttributeLists.ToList();
                    var nsName = string.Empty;

                    //(classDeclarationSyntax.Parent as Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax).Name.ToString()
                    if (classDeclarationSyntax.Parent is NamespaceDeclarationSyntax ns)
                    {
                        nsName = ns.Name.ToString();
                        if (!namespaces.Contains(nsName))
                        {
                            namespaces.Add(nsName);
                        }
                    }

                    foreach (var attr in attrs)
                    {
                        //屏蔽JustAsService
                        if (attrs.Any(x => x.Attributes.Any(x => x.Name.ToString() == QuickApiType.JustAsServiceTypeName)))
                        {
                            continue;
                        }

                        attr.Attributes.ToList().ForEach(x =>
                        {
                            var name = x.Name.ToString();
                            var args = x.ArgumentList?.Arguments.ToList();
                            if (name == QuickApiType.TypeName)
                            {
                                //路由地址
                                var route = args.First().Expression.ToString().ToRaw();
                                //验证策略
                                var policy = args.FirstOrDefault(x =>
                                x.NameEquals?.Name.ToString() == nameof(QuickApiType.Policy))?.Expression.ToString().ToRaw();
                                //请求类型
                                var verbs = ToVerbs(args.FirstOrDefault(x =>
                                x.NameEquals?.Name.ToString() == nameof(QuickApiType.Verbs))?.Expression.ToString().ToRaw());
                                //分组
                                var group = args.FirstOrDefault(x =>
                                x.NameEquals?.Name.ToString() == nameof(QuickApiType.Group))?.Expression.ToString().ToRaw();
                                //"GET","POST"
                                var verbsStr = string.Join(",", verbs.Select(x => $"\"{x}\""));

                                var source = routeTemp.Replace("$0", $"{group}/{route}")
                                    .Replace("$1", verbsStr)
                                    .Replace("$2", policy ?? "")
                                    .Replace("$3", string.IsNullOrEmpty(nsName) ? fullname : $"{nsName}.{fullname}")
                                    .Replace("$4", $"map{Guid.NewGuid().ToString().Substring(0, 8)}");

                                sb.AppendLine(source);

                            }
                        });
                    }
                }

                var namespacesSyntaxs = namespaces.Select(x => $"using {x};").ToList();

                var endpointSource = endpointTemp
                    .Replace("$version", typeof(QuickApiSourceGenerator).Assembly.GetName().Version?.ToString() ?? "")
                    .Replace("$namespace", string.Join(Environment.NewLine, namespacesSyntaxs))
                    .Replace("$apis", sb.ToString());

                ctx.AddSource($"QuickApiExtentions.g.cs", SourceText.From(endpointSource, Encoding.UTF8));

                #endregion

            });

        }
    }
}