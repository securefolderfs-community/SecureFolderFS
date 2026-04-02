// Some parts of the following code were used from WinUI3Utilities on the MIT License basis.
// See the associated license file for more information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static SecureFolderFS.SourceGenerator.Helpers.SourceGeneratorHelpers;

namespace SecureFolderFS.SourceGenerator
{
    [Generator(LanguageNames.CSharp)]
    public sealed class InjectGenerator : AttributeWithTypeGenerator
    {
        private const string LoggerFullName = "Microsoft.Extensions.Logging.ILogger";
        private const string LoggerFactoryFullName = "Microsoft.Extensions.Logging.ILoggerFactory";

        protected override string AttributeNamespace { get; } = $"{nameof(SecureFolderFS)}.Sdk.Attributes.InjectAttribute`1";

        protected override string? GetCode(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributes)
        {
            var members = new List<MemberDeclarationSyntax>();

            var getter = GetGetter();
            var serviceProviderProperty = GetPropertyDeclaration(SyntaxKind.PrivateKeyword, Constants.ServiceProviderName, Constants.ServiceProviderNamespace, getter)
                .AddAttributeLists(GetAttributeForMethod(Constants.AssemblyName, Constants.AssemblyVersion, nameof(SecureFolderFS)));

            members.Add(serviceProviderProperty);

            foreach (var attribute in attributes)
            {
                var name = string.Empty;
                var visibility = SyntaxKind.None;

                if (attribute.AttributeClass is not { TypeArguments: [var type, ..] })
                    return null;

                foreach (var namedArgument in attribute.NamedArguments)
                {
                    if (namedArgument.Value.Value is { } value)
                    {
                        switch (namedArgument.Key)
                        {
                            case "Name":
                                name = (string)value;
                                break;

                            case "Visibility":
                                visibility = GetVisibility((string)value);
                                break;
                        }
                    }
                }

                visibility = visibility == SyntaxKind.None ? SyntaxKind.PrivateKeyword : visibility;

                // Check if this is the special ILogger injection case
                var isLoggerInjection = type is INamedTypeSymbol { Name: "ILogger", IsGenericType: false } && type.ContainingNamespace.ToDisplayString().Contains("Microsoft.Extensions.Logging");
                if (isLoggerInjection)
                {
                    name = string.IsNullOrEmpty(name) ? "Logger" : name;
                    var backingFieldName = $"_{name}";
                    var containingTypeName = typeSymbol.ToDisplayString();
                    var loggerTypeName = $"global::Microsoft.Extensions.Logging.ILogger<{containingTypeName}>";

                    var loggerField = GetFieldDeclaration(SyntaxKind.PrivateKeyword, backingFieldName, loggerTypeName, true);
                    var loggerProperty = GetPropertyDeclaration(visibility, name, loggerTypeName).WithExpressionBody(
                        ArrowExpressionClause(
                            AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                                GetThisMemberAccessExpression(backingFieldName),
                                GetLoggerRegistration(containingTypeName, Constants.ServiceProviderName))))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                        .AddAttributeLists(GetAttributeForMethod(Constants.AssemblyName, Constants.AssemblyVersion, nameof(InjectGenerator)));

                    members.Add(loggerField);
                    members.Add(loggerProperty);
                }
                else
                {
                    name = string.IsNullOrEmpty(name) ? FormatName(type.Name) : name;
                    var backingFieldName = $"_{name}";

                    var injecteeField = GetFieldDeclaration(SyntaxKind.PrivateKeyword, backingFieldName, type.ToDisplayString(), true);
                    var injecteeProperty = GetPropertyDeclaration(visibility, name, type.ToDisplayString()).WithExpressionBody(
                        ArrowExpressionClause(
                            AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                                GetThisMemberAccessExpression(backingFieldName),
                                GetServiceRegistration(type, Constants.ServiceProviderName))))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                        .AddAttributeLists(GetAttributeForMethod(Constants.AssemblyName, Constants.AssemblyVersion, nameof(InjectGenerator)));

                    members.Add(injecteeField);
                    members.Add(injecteeProperty);
                }
            }

            if (members.Count > 0)
            {
                var generatedClass = GetClassDeclaration(typeSymbol, members);
                var generatedNamespace = GetFileScopedNamespaceDeclaration(typeSymbol, generatedClass);
                var compilationUnit = GetCompilationUnit(generatedNamespace);
                var source = SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText().ToString();

                return source;
            }

            return null;
        }

        private static SyntaxKind GetVisibility(string visibility)
        {
            return visibility switch
            {
                "public" => SyntaxKind.PublicKeyword,
                "protected" => SyntaxKind.ProtectedKeyword,
                "private" => SyntaxKind.PrivateKeyword,
                _ => SyntaxKind.PrivateKeyword
            };
        }

        private static string FormatName(string name)
        {
            if (name.Length < 2)
                return name;

            if (name[0] == 'I' && char.IsUpper(name[1]))
            {
                // Dealing with an interface, return a substring without the 'I'
                return name.Substring(1);
            }

            return name;
        }
    }
}
