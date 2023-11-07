// <copyright file="QuickApiSourceGenAnalyzer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Biwen.QuickApi.SourceGenerator
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class QuickApiSourceGenAnalyzer : DiagnosticAnalyzer
    {
        private const string Helplink = "https://github.com/vipwan/Biwen.QuickApi";
        private const string QuickApiMetaname = "IQuickApi`2";
        private const string QuickApiAttributeName = "QuickApiAttribute";

        /// <summary>
        /// 没有标记[QuickApi]特性的类.
        /// </summary>
#pragma warning disable RS2008 // 启用分析器发布跟踪
        private static readonly DiagnosticDescriptor NoMarkedAttribute = new(
            id: "GEN003",
#pragma warning restore RS2008 // 启用分析器发布跟踪
            title: "QuickApi类没有标记[QuickApi]特性",
            messageFormat: "当前QuickApi类没有标记[QuickApi]特性,将无法生成路由,确定要这样做!?",
            category: typeof(QuickApiSourceGenerator).Assembly.GetName().Name,
            DiagnosticSeverity.Warning,
            helpLinkUri: Helplink,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoMarkedAttribute);

        public override void Initialize(AnalysisContext context)
        {
            if (context == null) return;
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // context.RegisterSymbolAction(action =>
            // {
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
            // }, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(
                action =>
            {
                var symbol = action.ContainingSymbol;
                if (symbol == null) return;
                if (symbol is INamedTypeSymbol namedTypeSymbol)
                {
                    if (namedTypeSymbol.IsAbstract)
                    {
                        return;
                    }

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
            },
                SyntaxKind.ClassDeclaration);
        }
    }
}