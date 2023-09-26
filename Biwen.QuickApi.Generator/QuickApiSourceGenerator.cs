
namespace Biwen.QuickApi.SourceGenerator
{
    using System;
    using System.Text;
    using Microsoft.CodeAnalysis;
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


            // retrieve the populated receiver 
            if (!(context.SyntaxContextReceiver is QuickApiSyntaxReceivers quickApiSyntaxReceivers))
                return;

            var classDeclarationSyntaxes = quickApiSyntaxReceivers.ClassDeclarationSyntaxes;

            var sourceFormat = @"
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            namespace Biwen.QuickApi.SourceGenerator.TestConsole
            {
                public static class EndpointExtentions
                {
                    public static IEndpointRouteBuilder MapQuickApis(this IEndpointRouteBuilder app)
                    {
                        $0
                        return app;
                    }
                }
            }
";

            var addFromat = @"

            app.MapGet(""/fromapi/$0"", async () =>
            {
                await Task.CompletedTask;
                return Results.Text(""fromapi"");
            });
";

            var sb = new StringBuilder();

            foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var fullname = classDeclarationSyntax.Identifier.ValueText;
                var source = addFromat.Replace("$0", fullname);
                sb.Append(source);
            }

            var source2 = sourceFormat.Replace("$0", sb.ToString());
            context.AddSource($"extentions.g.cs", SourceText.From(source2, Encoding.UTF8));

            // Find the main method
            //var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken)!;
        }


        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new QuickApiSyntaxReceivers());
            //context.RegisterForPostInitialization((i) =>
            //{
            //    i.AddSource("test", "public class Test { }");
            //});

        }
    }
}