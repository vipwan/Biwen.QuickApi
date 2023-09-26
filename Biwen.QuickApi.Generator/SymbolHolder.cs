
namespace Biwen.QuickApi.SourceGenerator
{
    using Microsoft.CodeAnalysis;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    internal sealed class SymbolHolder
    {
        public INamedTypeSymbol RestApiAttribute { get; set; } = null!;
        public INamedTypeSymbol? JustAsServiceAttribute { get; set; }
        public INamedTypeSymbol? AliasAsAttribute { get; set; }

    }
}