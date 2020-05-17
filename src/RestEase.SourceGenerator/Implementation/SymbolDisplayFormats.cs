using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace RestEase.SourceGenerator.Implementation
{
    internal static class SymbolDisplayFormats
    {
        public static SymbolDisplayFormat TypeLookup { get; }
        public static SymbolDisplayFormat Namespace { get; }
        public static SymbolDisplayFormat ClassDeclaration { get; }
        public static SymbolDisplayFormat ImplementedInterface { get; }
        public static SymbolDisplayFormat MethodDeclaration { get; }
        public static SymbolDisplayFormat MethodReturnType { get; }

        static SymbolDisplayFormats()
        {
            TypeLookup = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            Namespace = new SymbolDisplayFormat();

            ClassDeclaration = new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
                    | SymbolDisplayGenericsOptions.IncludeTypeConstraints
                    | SymbolDisplayGenericsOptions.IncludeVariance);

            ImplementedInterface = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

            MethodDeclaration = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
                    | SymbolDisplayGenericsOptions.IncludeTypeConstraints
                    | SymbolDisplayGenericsOptions.IncludeVariance,
                memberOptions: SymbolDisplayMemberOptions.IncludeParameters
                    | SymbolDisplayMemberOptions.IncludeConstantValue
                    | SymbolDisplayMemberOptions.IncludeRef,
                parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut
                    | SymbolDisplayParameterOptions.IncludeType
                    | SymbolDisplayParameterOptions.IncludeName
                    | SymbolDisplayParameterOptions.IncludeDefaultValue,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                    | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

            MethodReturnType = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
        }
    }
}
