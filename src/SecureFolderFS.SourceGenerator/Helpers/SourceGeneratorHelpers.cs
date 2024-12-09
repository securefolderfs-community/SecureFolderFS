// Some parts of the following code were used from WinUI3Utilities on the MIT License basis.
// See the associated license file for more information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SecureFolderFS.SourceGenerator.Helpers
{
    internal static class SourceGeneratorHelpers
    {
        /// <summary>
        /// Generate the following code:
        /// <code>
        /// get;
        /// </code>
        /// </summary>
        /// <returns><see cref="AccessorDeclarationSyntax"/></returns>
        internal static AccessorDeclarationSyntax GetGetter() =>
            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        /// <summary>
        /// Generate the following code:
        /// <code>
        /// <paramref name="visibility"/> <paramref name="type" /> <paramref name="propertyName" /> <paramref name="accessors"/>;
        /// </code>
        /// </summary>
        /// <returns><see cref="PropertyDeclarationSyntax"/></returns>
        internal static PropertyDeclarationSyntax GetPropertyDeclaration(SyntaxKind visibility, string propertyName, string type, params AccessorDeclarationSyntax[] accessors) =>
            PropertyDeclaration(ParseTypeName(type), propertyName)
                .AddModifiers(Token(visibility))
                .AddAccessorListAccessors(accessors);

        /// <summary>
        /// Generate the following code:
        /// <code>
        /// <paramref name="visibility"/> <paramref name="type" /> <paramref name="propertyName" />
        /// </code>
        /// </summary>
        /// <returns><see cref="PropertyDeclarationSyntax"/></returns>
        internal static PropertyDeclarationSyntax GetPropertyDeclaration(SyntaxKind visibility, string propertyName, string type) =>
            PropertyDeclaration(ParseTypeName(type), propertyName)
                .AddModifiers(Token(visibility));

        /// <summary>
        /// Generate the following code:
        /// <code>
        /// <paramref name="visibility"/> <paramref name="type"/>*<paramref name="isNullable"/>* <paramref name="fieldName" />;
        /// </code>
        /// </summary>
        /// <returns>StaticFieldDeclaration</returns>
        internal static FieldDeclarationSyntax GetFieldDeclaration(SyntaxKind visibility, string fieldName, string type, bool isNullable) =>
            FieldDeclaration(VariableDeclaration(ParseNullableType(type, isNullable)))
                .AddModifiers(Token(visibility))
                .AddDeclarationVariables(VariableDeclarator(fieldName)).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        /// <summary>
        /// Generate the following code:
        /// <code>
        /// partial class <paramref name="specificType"/>&lt;<see cref="INamedTypeSymbol.TypeParameters"/>&gt;<br/>
        /// {
        ///     <paramref name="members" /><br/>
        /// }
        /// </code>
        /// </summary>
        /// <returns><see cref="ClassDeclarationSyntax"/></returns>
        internal static ClassDeclarationSyntax GetClassDeclaration(INamedTypeSymbol specificType, IList<MemberDeclarationSyntax> members)
        {
            for (var i = 0; i < members.Count - 1; i++)
                members[i] = members[i].WithTrailingTrivia(SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n"));

            var name = specificType.Name.Contains('`') ? specificType.Name.Substring(specificType.Name.Length-2) : specificType.Name;
            var classDeclarationSyntax = ClassDeclaration(name).AddModifiers(Token(SyntaxKind.PartialKeyword));

            if (!specificType.TypeParameters.IsEmpty)
            {
                classDeclarationSyntax = classDeclarationSyntax.AddTypeParameterListParameters(TypeParameter("TFolder"));

                var typeParameters = new SeparatedSyntaxList<TypeParameterSyntax>();
                foreach (var item in specificType.TypeParameters)
                {
                    typeParameters.Add(TypeParameter(item.Name));
                }

                classDeclarationSyntax = classDeclarationSyntax.WithTypeParameterList(TypeParameterList(
                        Token(SyntaxKind.LessThanToken),
                        typeParameters,
                        Token(SyntaxKind.GreaterThanToken)));
            }

            return classDeclarationSyntax.AddMembers(members.ToArray());
        }

        /// <summary>
        /// Generate the following code:
        /// <code>
        /// #pragma warning disable
        /// #nullable enable
        /// namespace <paramref name="specificClass" />.ContainingNamespace;<br/>
        /// <paramref name="generatedClass" />
        /// </code>
        /// </summary>
        /// <returns><see cref="FileScopedNamespaceDeclarationSyntax"/></returns>
        internal static FileScopedNamespaceDeclarationSyntax GetFileScopedNamespaceDeclaration(ISymbol specificClass, MemberDeclarationSyntax generatedClass) =>
            FileScopedNamespaceDeclaration(ParseName(specificClass.ContainingNamespace.ToDisplayString()))
                .AddMembers(generatedClass)
                .WithNamespaceKeyword(Token(SyntaxKind.NamespaceKeyword)
                    .WithLeadingTrivia(Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)))
                    .WithLeadingTrivia(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)))
                    );

        /// <summary>
        /// Generate the following code:
        /// <code>
        /// using Microsoft.UI.Xaml;
        /// ...
        /// <br/><paramref name="generatedNamespace" />
        /// </code>
        /// </summary>
        /// <returns><see cref="CompilationUnitSyntax"/></returns>
        internal static CompilationUnitSyntax GetCompilationUnit(MemberDeclarationSyntax generatedNamespace) =>
            CompilationUnit()
                .AddMembers(generatedNamespace)
                .NormalizeWhitespace();

        /// <summary>
        /// Generate the following code:
        /// <code>
        /// global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions&lt;<paramref name="injectionType"/>&gt;(<paramref name="serviceProviderName"/>);
        /// </code>
        /// </summary>
        /// <returns><see cref="ExpressionSyntax"/></returns>
        internal static ExpressionSyntax GetServiceRegistration(ITypeSymbol injectionType, string serviceProviderName) =>
            InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions"),
                    GenericName("GetRequiredService").WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>().Add(ParseTypeName(injectionType.ToDisplayString()))))))
                .AddArgumentListArguments(Argument(GetThisMemberAccessExpression(serviceProviderName)));

        /// <summary>
        /// Generate the following code:
        /// <code>
        /// [global::System.CodeDom.Compiler.GeneratedCode(<paramref name="assemblyName"/> + <paramref name="generatorName"/>, <paramref name="assemblyVersion"/>)]
        /// [global::System.Diagnostics.DebuggerNonUserCode]
        /// [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        /// </code>
        /// </summary>
        /// <returns>An array of <see cref="AttributeListSyntax"/></returns>
        internal static AttributeListSyntax[] GetAttributeForMethod(string assemblyName, string assemblyVersion, string generatorName) =>
            new[]
            {
                AttributeList().AddAttributes(Attribute(IdentifierName("global::System.CodeDom.Compiler.GeneratedCode"))
                    .AddArgumentListArguments(
                        AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal($"{assemblyName}.{generatorName}"))),
                        AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(assemblyVersion)))
                    )),
                AttributeList().AddAttributes(Attribute(IdentifierName("global::System.Diagnostics.DebuggerNonUserCode"))),
                AttributeList().AddAttributes(Attribute(IdentifierName("global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage")))
            };

        /// <summary>
        /// Generate the following code:
        /// <code>
        /// this.<paramref name="name"/>
        /// </code>
        /// </summary>
        /// <returns><see cref="MemberAccessExpressionSyntax"/></returns>
        internal static MemberAccessExpressionSyntax GetThisMemberAccessExpression(string name)
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                ThisExpression(),
                IdentifierName(name));
        }

        private static TypeSyntax ParseNullableType(string str, bool isNullable)
        {
            return isNullable ? NullableType(ParseTypeName(str)) : ParseTypeName(str);
        }
    }
}
