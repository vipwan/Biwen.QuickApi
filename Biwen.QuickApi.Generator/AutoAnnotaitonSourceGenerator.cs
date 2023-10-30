using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biwen.QuickApi.SourceGenerator
{

    //https://github.com/dotnet/extensions/blob/89ba9815bf2bf672090a26ebab7f3c2615ec404c/src/Generators/Microsoft.Gen.EnumStrings/EnumStringsGenerator.cs


    [Generator(LanguageNames.CSharp)]
#pragma warning disable RS1036 // 指定分析器禁止的 API 强制设置
    public class AutoAnnotaitonSourceGenerator : IIncrementalGenerator
#pragma warning restore RS1036 // 指定分析器禁止的 API 强制设置
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var attrs = context.SyntaxProvider.ForAttributeWithMetadataName(
                  "Biwen.QuickApi.Attributes.AutoAnnotationAttribute",
                  (context, attributeSyntax) => true,
                  (syntaxContext, _) => syntaxContext).Collect();


            context.RegisterSourceOutput(attrs, (ctx, source) =>
            {

                foreach (var attr in source.AsEnumerable())
                {
                    

                }


            });


            return;














            // auto annotation
            var autoAnnotationProvider = context.SyntaxProvider.CreateSyntaxProvider(
                 (syntax, cancellationToken) => syntax is InterfaceDeclarationSyntax
                 {
                 },
                 (context, cancellationToken) =>
                 (InterfaceDeclarationSyntax)context.Node)
                      .Where(x => x.AttributeLists.Count > 0)
                      .Where(x => x.AttributeLists.SelectMany(x => x.Attributes)
                      .Any(x => x.Name.ToString() == QuickApiType.AutoAnnotationName)).Collect();


            Func<string, IncrementalValueProvider<ImmutableArray<ClassDeclarationSyntax>>> GetAnnotationClass = (interfaceName) =>
            {
                var csp = context.SyntaxProvider.CreateSyntaxProvider(
                     (syntax, cancellationToken) => syntax is ClassDeclarationSyntax
                     { },
                     (context, cancellationToken) =>
                     (ClassDeclarationSyntax)context.Node)
                     .Where(x => x.Parent != null && x.Parent.ToFullString().Contains(interfaceName)).Collect();

                return csp;
            };


            IList<string> Interfaces = new List<string>();



            IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();



            context.RegisterSourceOutput(autoAnnotationProvider, (ctx, source) =>
            {
                #region 生成 auto annotation
                foreach (var interfaceDeclarationSyntax in source.AsEnumerable())
                {



                    //获取到属性定义
                    //(interfaceDeclarationSyntax.ChildNodes().Where(x=>x is PropertyDeclarationSyntax).ToList()[0] as PropertyDeclarationSyntax)
                    //获取属性类型 : string?
                    //.Type.GetText() 
                    //获取属性名称 : Hello
                    //.Identifier.ValueText
                    //获取属性所有特性 : [Description(\"hello\")]\r\n [DefaultValue(\"hello\")]\r\n
                    //.AttributeLists.ToFullString()

                    var props = (interfaceDeclarationSyntax.ChildNodes().Where(x => x is PropertyDeclarationSyntax).ToList());

                    string temp = $@"
        $annotation
        public $propertyType $propertyName {{ get; set; }}
";

                    string classTemp = $@"
namespace $namespace
{{
    public partial class $className
    {{
        $props
    }}
}}";


                    StringBuilder stringBuilder = new StringBuilder();



                    foreach (PropertyDeclarationSyntax prop in props)
                    {
                        var body = temp
                        .Replace("$annotation", prop!.AttributeLists.ToFullString().Replace("\r\n", "").Replace(" ", ""))
                        .Replace("$propertyType", prop!.Type.ToString())
                        .Replace("$propertyName", prop!.Identifier.ValueText);

                        stringBuilder.AppendLine(body);
                    }

                    var propsText = stringBuilder.ToString();

                    var classes = classTemp
                    .Replace("$namespace", "Hello")
                    .Replace("$className", "AutoClass")
                    .Replace("$props", propsText);

                    keyValuePairs.Add(interfaceDeclarationSyntax.Identifier.ValueText, classes);

                    //Interfaces.Add(interfaceDeclarationSyntax.Identifier.ValueText);
                }


                //foreach (var item in Interfaces.Distinct())
                //{
                //    var classes = GetAnnotationClass(item);

                //    context.RegisterSourceOutput(classes, (ctx, source) =>
                //    {
                //        #region 生成 auto annotation
                //        foreach (var classDeclarationSyntax in source.AsEnumerable())
                //        {

                //            //todo:
                //        }
                //        #endregion
                //    });

                //}

                #endregion
            });


            foreach (var kv in keyValuePairs)
            {
                var classes = GetAnnotationClass(kv.Key);


                context.RegisterSourceOutput(classes, (ctx, source) =>
                {
                    //todo:







                });


            }
        }
    }
}