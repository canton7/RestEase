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
        public static SymbolDisplayFormat PropertyDeclaration { get; }
        public static SymbolDisplayFormat MethodDeclaration { get; }
        public static SymbolDisplayFormat MethodReturnType { get; }
        public static SymbolDisplayFormat ParameterReference { get; }
        public static SymbolDisplayFormat PropertyReference { get; }

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

            PropertyDeclaration = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions: SymbolDisplayMemberOptions.IncludeType,
                propertyStyle: SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

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

            ParameterReference = new SymbolDisplayFormat(
                parameterOptions: SymbolDisplayParameterOptions.IncludeName,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            PropertyReference = new SymbolDisplayFormat(
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
        }
    }
}
