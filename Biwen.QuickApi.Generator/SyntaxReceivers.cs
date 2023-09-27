
namespace Biwen.QuickApi.SourceGenerator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Linq;

#if !SHARED_PROJECT
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
    internal class QuickApiSyntaxReceivers : ISyntaxContextReceiver
    {

        internal static ISyntaxContextReceiver Create() => new QuickApiSyntaxReceivers();

        public List<ClassDeclarationSyntax> ClassDeclarationSyntaxes { get; } = new();

        /// <summary>
        /// QuickApi
        /// </summary>
        const string QuickApiAttributeName = "QuickApi";

        /// <summary>
        /// 查找所有标记[QuickApi]的接口
        /// </summary>
        /// <param name="context"></param>
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
            {
                if (classDeclarationSyntax.AttributeLists.Count == 0) { return; }

                var quickApiAttribute = classDeclarationSyntax.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .FirstOrDefault(x => x.Name.ToString() == QuickApiAttributeName);

                if (quickApiAttribute != null)
                {
                    //TestQuickApi
                    var fullname = classDeclarationSyntax.Identifier.ValueText;
                    //QuickApi(\"test1\")
                    var fname2 = quickApiAttribute.ToFullString();
                    //QuickApi
                    var name = quickApiAttribute.Name.ToString();

                    ClassDeclarationSyntaxes.Add(classDeclarationSyntax);
                }
            }
        }
    }
}