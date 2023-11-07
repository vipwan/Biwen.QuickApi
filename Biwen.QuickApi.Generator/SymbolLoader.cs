using Microsoft.CodeAnalysis;

namespace Biwen.QuickApi.SourceGenerator
{
    internal static class SymbolLoader
    {
        internal const string RestApiAttribute = "Biwen.QuickApi.Attributes.QuickApiAttribute";

        internal const string JustAsServiceAttribute = "Biwen.QuickApi.Attributes.JustAsServiceAttribute";
        internal const string AliasAsAttribute = "Biwen.QuickApi.Attributes.AliasAsAttribute";

        /// <summary>
        /// 加载所有的符号
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
        internal static SymbolHolder? LoadSymbols(Compilation compilation)
        {
            var restApiAttribute = compilation.GetTypeByMetadataName(RestApiAttribute);

            var justAsServiceAttribute = compilation.GetTypeByMetadataName(JustAsServiceAttribute);
            var aliasAsAttribute = compilation.GetTypeByMetadataName(AliasAsAttribute);

            if (restApiAttribute == null)
            {
                // nothing to do if these types aren't available
                return null;
            }

            return new SymbolHolder
            {
                AliasAsAttribute = aliasAsAttribute,
                JustAsServiceAttribute = justAsServiceAttribute,
                RestApiAttribute = restApiAttribute,
            };
        }
    }
}