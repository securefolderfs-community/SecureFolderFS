// Some parts of the following code were used from WinUI3Utilities on the MIT License basis.
// See the associated license file for more information.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace SecureFolderFS.SourceGenerator
{
    public abstract class AttributeWithTypeGenerator : IIncrementalGenerator
    {
        protected abstract string AttributeNamespace { get; }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var generatorAttributes = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeNamespace,
                static (_, _) => true,
                (syntaxContext, _) => syntaxContext
            ).Combine(context.CompilationProvider);

            context.RegisterSourceOutput(generatorAttributes, (spc, leftRight) =>
            {
                if (leftRight.Left.TargetSymbol is not INamedTypeSymbol typeSymbol)
                    return;

                if (GetCode(typeSymbol, leftRight.Left.Attributes) is not { } source)
                    return;

                var fileName = $"{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}_{AttributeNamespace}.g.cs";
                spc.AddSource(fileName, source);
            });
        }

        protected abstract string? GetCode(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributes);
    }
}
