using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

namespace Biwen.QuickApi.SourceGenerator
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning disable RS1036 // 指定分析器禁止的 API 强制设置
    public class QuickApiSourceGenAnalyzer : DiagnosticAnalyzer
#pragma warning restore RS1036 // 指定分析器禁止的 API 强制设置
    {
        const string helplink = "https://github.com/vipwan/Biwen.QuickApi";

        const string QuickApiMetaname = "IQuickApi`2";
        const string QuickApiAttributeName = "QuickApiAttribute";

        /// <summary>
        /// 没有标记[QuickApi]特性的类
        /// </summary>
#pragma warning disable RS2008 // 启用分析器发布跟踪
        private static readonly DiagnosticDescriptor NoMarkedAttribute = new(id: "GEN003",
#pragma warning restore RS2008 // 启用分析器发布跟踪
                                                                              title: "QuickApi类没有标记[QuickApi]特性",
#pragma warning disable RS1032 // 正确定义诊断消息
                                                                              messageFormat: "当前QuickApi类没有标记[QuickApi]特性,将无法生成路由,确定要这样做!?",
#pragma warning restore RS1032 // 正确定义诊断消息
                                                                              category: typeof(QuickApiSourceGenerator).Assembly.GetName().Name,
                                                                              DiagnosticSeverity.Warning,
                                                                              helpLinkUri: helplink,
                                                                              isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoMarkedAttribute);

        public override void Initialize(AnalysisContext context)
        {
            if (context == null) return;
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            //context.RegisterSymbolAction(action =>
            //{
            //    var symbol = action.Symbol;
            //    if (symbol is INamedTypeSymbol namedTypeSymbol)
            //    {
            //        if (namedTypeSymbol.IsAbstract) { return; }
            //        if (namedTypeSymbol.AllInterfaces.Length == 0)
            //        {
            //            return;
            //        }
            //        if (namedTypeSymbol.AllInterfaces.Any(x => x.MetadataName == QuickApiMetaname))
            //        {
            //            if (!namedTypeSymbol.GetAttributes().Any(x => x.AttributeClass?.Name == QuickApiAttributeName))
            //            {
            //                action.ReportDiagnostic(Diagnostic.Create(NoMarkedAttribute, symbol.Locations[0], helplink));
            //            }
            //        }
            //    }
            //}, SymbolKind.NamedType);

            context.RegisterSyntaxNodeAction(action =>
            {
                var symbol = action.ContainingSymbol;
                if (symbol == null) return;
                if (symbol is INamedTypeSymbol namedTypeSymbol)
                {
                    if (namedTypeSymbol.IsAbstract) { return; }
                    if (namedTypeSymbol.AllInterfaces.Length == 0)
                    {
                        return;
                    }
                    if (namedTypeSymbol.AllInterfaces.Any(x => x.MetadataName == QuickApiMetaname))
                    {
                        if (!namedTypeSymbol.GetAttributes().Any(x => x.AttributeClass?.Name == QuickApiAttributeName))
                        {
                            var location = action.Node.GetLocation();
                            action.ReportDiagnostic(Diagnostic.Create(NoMarkedAttribute, location));
                        }
                    }
                }
            }, SyntaxKind.ClassDeclaration);
        }
    }
}