namespace Biwen.QuickApi.SourceGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    //using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    //using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [Generator(LanguageNames.CSharp)]
#pragma warning disable RS1036 // 指定分析器禁止的 API 强制设置
    public class QuickApiSourceGenerator : IIncrementalGenerator
#pragma warning restore RS1036 // 指定分析器禁止的 API 强制设置
    {
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


        #region DiagnosticDescriptor

        /// <summary>
        /// JustAsService信息
        /// </summary>
#pragma warning disable RS2008 // 启用分析器发布跟踪
        private static readonly DiagnosticDescriptor JustAsServiceInformation = new(id: "GEN002",
#pragma warning restore RS2008 // 启用分析器发布跟踪
                                                                              title: "来自Biwen.QuickApi.SourceGenerator的信息",
#pragma warning disable RS1032 // 正确定义诊断消息
                                                                              messageFormat: "标注特性[JustAsService]的QuickApi将不会生成路由.",
#pragma warning restore RS1032 // 正确定义诊断消息
                                                                              category: typeof(QuickApiSourceGenerator).Assembly.GetName().Name,
                                                                              DiagnosticSeverity.Warning,
                                                                              isEnabledByDefault: true);


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

            var compilationAndTypes = context.CompilationProvider.Combine(quickApiProvider);

            context.RegisterSourceOutput(compilationAndTypes, (ctx, source) =>
            {
                var compilation = source.Left;
                var types = source.Right;

                #region 生成QuickApi代码

                var sb = new StringBuilder();

                //存储所有的命名空间
                IList<string> namespaces = new List<string>();

                //存储所有的路由信息
                List<(string? Group, RouteInfo Info)> groups = new();

                foreach (var classDeclarationSyntax in types.AsEnumerable())
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
                            ctx.ReportDiagnostic(Diagnostic.Create(JustAsServiceInformation, attr.GetLocation()));
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

                                groups.Add((group, new RouteInfo
                                {
                                    Route = route,
                                    Verb = verbsStr,
                                    Policy = policy,
                                    NameSpace = string.IsNullOrEmpty(nsName) ? fullname : $"{nsName}.{fullname}",
                                    ClassName = $"{Guid.NewGuid().ToString().Substring(0, 8)}"
                                }));
                            }
                        });
                    }
                }

                StringBuilder groupRoot = new();

                foreach (var group in groups.GroupBy(x => x.Group))
                {
                    var groupX = Guid.NewGuid().ToString().Substring(0, 8);

                    var source1 = GroupTemp
                    .Replace("$0", group.Key ?? string.Empty)
                    .Replace("$1", groupX);

                    StringBuilder groupSb = new StringBuilder();

                    foreach (var item in group)
                    {
                        string routeString = RouteTemp
                        .Replace("$4", groupX)
                        .Replace("$0", item.Info.ClassName)
                        .Replace("$1", item.Info.Route)
                        .Replace("$2", item.Info.Verb)
                        .Replace("$3", item.Info.NameSpace)
                        .Replace("$5", item.Info.Policy);

                        groupSb.AppendLine(routeString);
                    }

                    var temp = groupSb.ToString();
                    source1 = source1.Replace("$apis", temp);
                    groupRoot.AppendLine(source1);
                }


                var namespacesSyntaxs = namespaces.Select(x => $"using {x};").ToList();


                var genx = RootTemp
                    .Replace("$version", typeof(QuickApiSourceGenerator).Assembly.GetName().Version?.ToString() ?? "")
                    .Replace("$namespace", string.Join(Environment.NewLine, namespacesSyntaxs))
                    .Replace("$apis", groupRoot.ToString());


                //var endpointSource = endpointTemp
                //    .Replace("$version", typeof(QuickApiSourceGenerator).Assembly.GetName().Version?.ToString() ?? "")
                //    .Replace("$namespace", string.Join(Environment.NewLine, namespacesSyntaxs))
                //    .Replace("$apis", sb.ToString());

                //ctx.AddSource($"QuickApiExtentions.g.cs", SourceText.From(endpointSource, Encoding.UTF8));

                //format:
                genx = FormatContent(genx);
                ctx.AddSource($"QuickApiExtentions.g.cs", SourceText.From(genx, Encoding.UTF8));

                #endregion
            });
        }

        /// <summary>
        /// 格式化代码
        /// </summary>
        /// <param name="csCode"></param>
        /// <returns></returns>
        private static string FormatContent(string csCode)
        {
            var tree = CSharpSyntaxTree.ParseText(csCode);
            var root = tree.GetRoot().NormalizeWhitespace();
            var ret = root.ToFullString();
            return ret;
        }


        #region template

        static string RootTemp = $@"

//code gen for Biwen.QuickApi
//version :$version
//author :vipwan@outlook.com 万雅虎
//https://github.com/vipwan/Biwen.QuickApi
//如果你在使用中遇到问题,请第一时间issue,谢谢!

using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using Biwen.QuickApi.Http;
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
    public static (string Group, RouteGroupBuilder RouteGroupBuilder)[] MapGenQuickApis(this IEndpointRouteBuilder app, IServiceProvider serviceProvider,string prefix = ""api"")
    {{
        prefix ??= string.Empty;
        //middleware:
        (app as WebApplication)?.UseMiddleware<QuickApiMiddleware>();

        var groupBuilder = app.MapGroup(prefix);
        using var scope = serviceProvider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<BiwenQuickApiOptions>>().Value;
        if (options.EnableAntiForgeryTokens)
        {{
            //middleware:
#if !NET8_0_OR_GREATER
            (app as WebApplication)?.UseMiddleware<QuickApiAntiforgeryMiddleware>();
#endif
#if NET8_0_OR_GREATER
        (app as WebApplication)?.UseAntiforgery();
#endif
        }}

        List<(string, RouteGroupBuilder)> groups = new();
        $apis
        return groups.ToArray();
    }}

    /// <summary>
    /// GroupBuilder
    /// </summary>
    /// <returns></returns>
    private static RouteGroupBuilder GroupBuilder(RouteGroupBuilder groupBuilder, IServiceProvider serviceProvider, string group)
    {{
        if (group == null) {{ return groupBuilder; }}
        groupBuilder = groupBuilder.MapGroup(group);

        //GroupRouteBuilder
        var groupRouteBuilders = serviceProvider.GetServices<IQuickApiGroupRouteBuilder>();
        foreach (var groupRouteBuilder in groupRouteBuilders.OrderBy(x => x.Order))
        {{
            if (groupRouteBuilder.Group.Equals(group, StringComparison.OrdinalIgnoreCase))
            {{
                groupBuilder = groupRouteBuilder.Builder(groupBuilder);
            }}
        }}
        return groupBuilder;
    }}

    /// <summary>
    /// 验证Policy
    /// </summary>
    /// <exception cref=""""QuickApiExcetion""""></exception>
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

        static string GroupTemp = $@"
        //step1
        {{
            //step2
            //group: $0
            var group$1 = GroupBuilder(groupBuilder, serviceProvider, ""$0"");
            {{
                //step3
                $apis
            }}
            groups.Add((""$0"", group$1));
        }}
";

        static string RouteTemp = $@"
                var map$0 = group$4.MapMethods(""$1"", new[] {{ $2 }}, async (IHttpContextAccessor ctx, $3 api) =>
                {{
                    var checkResult = await CheckPolicy(ctx, ""$5"");
                    if (!checkResult.Flag) return checkResult.Result!;
                    try
                    {{
                        var req = await api.ReqBinder.BindAsync(ctx.HttpContext!);
                        var vresult = req.Validate();
                        if (!vresult.IsValid) {{ return TypedResults.ValidationProblem(vresult.ToDictionary()); }}
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
                map$0.WithMetadata(new QuickApiMetadata(typeof($3)));

                //antiforgery
                //net8.0以上使用UseAntiforgery,
                //net7.0以下使用QuickApiAntiforgeryMiddleware
                var antiforgeryApi$0 = scope.ServiceProvider.GetRequiredService(typeof($3)) as IAntiforgeryApi;
#if NET8_0_OR_GREATER
                if (antiforgeryApi$0?.IsAntiforgeryEnabled is false)
                {{
                    map$0.DisableAntiforgery();
                }}
                if (antiforgeryApi$0?.IsAntiforgeryEnabled is true)
                {{
                    if (!options.EnableAntiForgeryTokens)
                        {{
                            throw new QuickApiExcetion($""如需要防伪验证,请启用BiwenQuickApiOptions.EnableAntiForgeryTokens!"");
                        }}
                    map$0.WithMetadata(new RequireAntiforgeryTokenAttribute(true));
                }}
#endif

                //handlerbuilder
                scope.ServiceProvider.GetRequiredService<$3>().HandlerBuilder(map$0);
                //outputcache
                var map$0Cache = typeof($3).GetCustomAttribute<OutputCacheAttribute>();
                if (map$0Cache != null) map$0.WithMetadata(map$0Cache);
                //endpointgroup
                var map$0EndpointgroupAttribute = typeof($3).GetCustomAttribute<EndpointGroupNameAttribute>();
                if (map$0EndpointgroupAttribute != null) map$0.WithMetadata(map$0EndpointgroupAttribute);
                //authorizeattribute
                var map$0authorizeAttributes = typeof($3).GetCustomAttributes<AuthorizeAttribute>();
                if (map$0authorizeAttributes.Any()) map$0.WithMetadata(new AuthorizeAttribute());
                foreach (var authAttr in map$0authorizeAttributes)
                {{
                    map$0.WithMetadata(authAttr);
                }}
                //allowanonymous
                var map$0allowanonymous = typeof($3).GetCustomAttribute<AllowAnonymousAttribute>();
                if (map$0allowanonymous != null) map$0.WithMetadata(map$0allowanonymous);
";

        #endregion

        private class RouteInfo
        {
            public string? Route { get; set; }
            public string? Verb { get; set; }
            public string? Policy { get; set; }
            public string? NameSpace { get; set; }
            public string? ClassName { get; set; }
        }
    }
}