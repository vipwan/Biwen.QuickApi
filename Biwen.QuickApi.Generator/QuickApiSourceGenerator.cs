
namespace Biwen.QuickApi.SourceGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [Generator]
#pragma warning disable RS1036 // 指定分析器禁止的 API 强制设置
    public class QuickApiSourceGenerator : ISourceGenerator
#pragma warning restore RS1036 // 指定分析器禁止的 API 强制设置
    {

        public void Execute(GeneratorExecutionContext context)
        {

            var symbos = SymbolLoader.LoadSymbols(context.Compilation);
            if (symbos == null) { return; }


            #region 生成QuickApi代码


            // retrieve the populated receiver 
            if (!(context.SyntaxContextReceiver is QuickApiSyntaxReceivers quickApiSyntaxReceivers))
                return;

            var classDeclarationSyntaxes = quickApiSyntaxReceivers.ClassDeclarationSyntaxes;

            var sb = new StringBuilder();

            //存储所有的命名空间
            IList<string> namespaces = new List<string> { context.Compilation.AssemblyName! };

            foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var fullname = classDeclarationSyntax.Identifier.ValueText;
                var attrs = classDeclarationSyntax.AttributeLists.ToList();

                //(classDeclarationSyntax.Parent as Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax).Name.ToString()

                if (classDeclarationSyntax.Parent is NamespaceDeclarationSyntax ns)
                {
                    var nsName = ns.Name.ToString();
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
                                .Replace("$3", fullname);

                            sb.AppendLine(source);

                        }
                    });
                }
            }

            var namespacesSyntaxs = namespaces.Select(x => $"using {x};").ToList();

            var endpointSource = endpointTemp.Replace(
                "$namespace", string.Join(Environment.NewLine, namespacesSyntaxs))
                .Replace("$apis", sb.ToString());

            context.AddSource($"QuickApiExtentions.g.cs", SourceText.From(endpointSource, Encoding.UTF8));

            // Find the main method
            //var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken)!;

            #endregion
        }

        #region template


        const string endpointTemp = $@"
//code gen for Biwen.QuickApi
//author :vipwan@outlook.com 万雅虎
//https://github.com/vipwan/Biwen.QuickApi
//如果你在使用中遇到问题,请第一时间issue,谢谢!

using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using Microsoft.Extensions.DependencyInjection;

$namespace

public static partial class AppExtentions
{{


    /// <summary>
    /// 源代码生成器的模板代码
    /// </summary>
    /// <param name=""app""></param>
    /// <param name=""group""></param>
    /// <returns></returns>
    /// <exception cref=""ArgumentNullException""></exception>
    /// <exception cref=""QuickApiExcetion""></exception>
    public static RouteGroupBuilder MapGenQuickApis(this IEndpointRouteBuilder app, IServiceProvider serviceProvider,string prefix = ""api"")
    {{
        if (string.IsNullOrEmpty(prefix))
        {{
            throw new ArgumentNullException(nameof(prefix));
        }}
        var groupBuilder = app.MapGroup(prefix);
        using var scope = serviceProvider.CreateScope();
        $apis
        return groupBuilder;
    }}
}}
";

        const string routeTemp = $@"
        var map$3 = groupBuilder.MapMethods(""$0"", new[] {{ $1 }}, async (IHttpContextAccessor ctx, $3 api) =>
            {{
                //验证策略
                var policy = ""$2"";
                if (!string.IsNullOrEmpty(policy))
                {{
                    var httpContext = ctx.HttpContext;
                    var authService = httpContext!.RequestServices.GetService<IAuthorizationService>() ?? throw new QuickApiExcetion($""IAuthorizationService is null,besure services.AddAuthorization() first!"");
                    var authorizationResult = await authService.AuthorizeAsync(httpContext.User, policy);
                    if (!authorizationResult.Succeeded)
                    {{
                        return Results.Unauthorized();
                    }}
                }}
                //绑定对象
                var req = await api.ReqBinder.BindAsync(ctx.HttpContext!);

                //验证器
                if (req.RealValidator.Validate(req) is ValidationResult vresult && !vresult!.IsValid)
                {{
                    return Results.ValidationProblem(vresult.ToDictionary());
                }}
                //执行请求
                try
                {{
                    var result = await api.ExecuteAsync(req!);
                    return Results.Json(result);
                }}
                catch (Exception ex)
                {{
                    var exceptionHandlers = ctx.HttpContext!.RequestServices.GetServices<IQuickApiExceptionHandler>();
                    //异常处理
                    foreach (var handler in exceptionHandlers)
                    {{
                        await handler.HandleAsync(ex);
                    }}
                    //默认处理
                    throw;
                }}
            }});

        //handler
        scope.ServiceProvider.GetRequiredService<$3>().HandlerBuilder(map$3);
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
            return list;
        }

        #endregion





        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => QuickApiSyntaxReceivers.Create());
            //context.RegisterForPostInitialization((i) =>
            //{
            //    i.AddSource("test", "public class Test { }");
            //});
        }
    }
}