using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CustomControlExample.SourceGenerators
{
    [Generator]
    class DependencyPropertyGenerator : IIncrementalGenerator
    {
        private struct DependencyPropertyInfo
        {
            public IPropertySymbol PropertySymbol { get; set; }
            public bool ManualSetDefaultValue { get; set; }
            public AttributeData AssociatedAttribute { get; set; }
        }

        private static IncrementalValuesProvider<T> Merge<T>(
            IncrementalValuesProvider<T> one,
            IncrementalValuesProvider<T> other)
        {
            return one.Collect()
                .Combine(other.Collect())
                .SelectMany((tuple, _) =>
                {
                    List<T> merged = new List<T>(tuple.Left.Length + tuple.Right.Length);
                    merged.AddRange(tuple.Left);
                    merged.AddRange(tuple.Right);
                    return merged;
                });
        }

        private static string GetPropertyLiteral(AttributeData attribute, string name)
        {
            foreach (KeyValuePair<string, TypedConstant> pair in attribute.NamedArguments)
            {
                if (pair.Key == name)
                {
                    return pair.Value.ToCSharpString();
                }
            }
            return null;
        }

        private static string GetAccessibilityLiteral(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Private:
                    return "private";
                case Accessibility.ProtectedAndInternal:
                    return "protected internal";
                case Accessibility.Protected:
                    return "protected";
                case Accessibility.Internal:
                    return "internal";
                case Accessibility.Public:
                    return "public";
                default:
                    break;
            }
            return string.Empty;
        }

        public void Initialize(IncrementalGeneratorInitializationContext initContext)
        {
#if DEBUG
            //Debugger.Launch();
#endif
            IncrementalValuesProvider<DependencyPropertyInfo> propertyInfosWithoutDefaultValueProvider = initContext.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "CustomControlExample.Controls.Attributes.DependencyPropertyAttribute",
                    (syntaxNode, _) => syntaxNode is PropertyDeclarationSyntax,
                    (syntaxContext, _) => new DependencyPropertyInfo
                    {
                        PropertySymbol = (IPropertySymbol) syntaxContext.TargetSymbol,
                        ManualSetDefaultValue = false
                    });

            IncrementalValuesProvider<DependencyPropertyInfo> propertyInfosWithDefaultValueProvider = initContext.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "CustomControlExample.Controls.Attributes.DependencyPropertyAttribute`1",
                    (syntaxNode, _) => syntaxNode is PropertyDeclarationSyntax,
                    (syntaxContext, _) => new DependencyPropertyInfo
                    {
                        PropertySymbol = (IPropertySymbol) syntaxContext.TargetSymbol,
                        ManualSetDefaultValue = true,
                        AssociatedAttribute = syntaxContext.Attributes[0]
                    });

            IncrementalValueProvider<ImmutableArray<DependencyPropertyInfo>> allPropertyInfosProvider = Merge(
                propertyInfosWithDefaultValueProvider,
                propertyInfosWithoutDefaultValueProvider
            ).Collect();

            initContext.RegisterSourceOutput(
                allPropertyInfosProvider,
                (sourceProductionContext, propertyInfos) =>
                {
                    Dictionary<INamedTypeSymbol, List<DependencyPropertyInfo>> propertyInfosOfClass = new Dictionary<INamedTypeSymbol, List<DependencyPropertyInfo>>(SymbolEqualityComparer.Default);

                    foreach (DependencyPropertyInfo propertyInfo in propertyInfos)
                    {
                        INamedTypeSymbol ownerClass = propertyInfo.PropertySymbol.ContainingType;
                        if (!propertyInfosOfClass.ContainsKey(ownerClass))
                        {
                            propertyInfosOfClass[ownerClass] = new List<DependencyPropertyInfo>();
                        }
                        propertyInfosOfClass[ownerClass].Add(propertyInfo);
                    }

                    foreach (INamedTypeSymbol classSymbol in propertyInfosOfClass.Keys)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append(@"
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
                        ");
                        stringBuilder.Append($@"
namespace {classSymbol.ContainingNamespace}
{{
    {GetAccessibilityLiteral(classSymbol.DeclaredAccessibility)} partial class {classSymbol.Name}
    {{
                        ");
                        foreach (DependencyPropertyInfo propertyInfo in propertyInfosOfClass[classSymbol])
                        {
                            string accessModifier = GetAccessibilityLiteral(propertyInfo.PropertySymbol.DeclaredAccessibility);
                            string propertyTypeName = propertyInfo.PropertySymbol.Type.Name;
                            string propertyName = propertyInfo.PropertySymbol.Name;
                            string ownerClass = propertyInfo.PropertySymbol.ContainingType.Name;

                            stringBuilder.Append($@"
        {accessModifier} partial {propertyTypeName} {propertyName}
        {{
            get => ({propertyTypeName}) GetValue({propertyName}Property);
            set => SetValue({propertyName}Property, value);
        }}
                            ");
                            if (propertyInfo.ManualSetDefaultValue)
                            {
                                stringBuilder.Append($@"
        {accessModifier} static readonly DependencyProperty {propertyName}Property = DependencyProperty.Register(
            nameof({propertyName}),
            typeof({propertyTypeName}),
            typeof({ownerClass}),
            new PropertyMetadata({GetPropertyLiteral(propertyInfo.AssociatedAttribute, "DefaultValue")})
        );
                                ");
                            }
                            else
                            {
                                stringBuilder.Append($@"
        {accessModifier} static readonly DependencyProperty {propertyName}Property = DependencyProperty.Register(
            nameof({propertyName}),
            typeof({propertyTypeName}),
            typeof({ownerClass}),
            new PropertyMetadata(default({propertyTypeName}))
        );
                                ");
                            }
                        }
                        stringBuilder.Append($@"
    }}
}}
                        ");
                        sourceProductionContext.AddSource($"{classSymbol.ContainingNamespace}.{classSymbol.Name}.g.cs", stringBuilder.ToString());
                    }
                }
            );
        }
    }
}
