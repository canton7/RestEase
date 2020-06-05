using Microsoft.CodeAnalysis;

namespace RestEase.SourceGenerator.Implementation
{
    internal static class SymbolDisplayFormats
    {
        /// <summary>
        /// The name of a symbol, suitably escaped. E.g. "List" or "MethodName"
        /// </summary>
        public static SymbolDisplayFormat SymbolName { get; }

        /// <summary>
        /// The full name of a namespace, suitably escaped. E.g. "System.Collections.Generic"
        /// </summary>
        public static SymbolDisplayFormat Namespace { get; }
        /// <summary>
        /// The name of a class, including type parameters, suitably escaped. E.g. "List&lt;T&gt;"
        /// </summary>
        public static SymbolDisplayFormat ClassName { get; }
        /// <summary>
        /// The fully-qualified name name of a class, including type parameters and constraints, suitably escaped.
        /// E.g. "global::My.Namespace.Foo&lt;T&gt; where T : global::System.Collections.Generic.IEnumerable&lt;T&gt;"
        /// </summary>
        public static SymbolDisplayFormat QualifiedClassNameWithTypeConstraints { get; }
        /// <summary>
        /// A type name, suitable for appending to a prefix for use as a constructor. No escaping.
        /// E.g. "IFoo".
        /// </summary>
        public static SymbolDisplayFormat ConstructorName { get; }
        /// <summary>
        /// The fully-qualified name of an interface that's being implemented, including type parameters (but excluding
        /// type constraints), suitably escaped. E.g. "global::My.Namespace.IFoo&lt;T&gt;"
        /// </summary>
        public static SymbolDisplayFormat ImplementedInterface { get; }
        /// <summary>
        /// A string suitable for use when declaring a property, suitably escaped. Includes getter/setter but doesn't
        /// include a return type or accessibility. E.g. "Foo { get; set; }"
        /// </summary>
        public static SymbolDisplayFormat PropertyDeclaration { get; }
        /// <summary>
        /// A string suitable for use when declaring a method, suitably escaped. Doesn't include return type or accessibility,
        /// but includes parameters, type constraints, default values. 
        /// E.g. "Foo<T>
        /// </summary>
        public static SymbolDisplayFormat ImplicitMethodDeclaration { get; }
        public static SymbolDisplayFormat ExplicitMethodDeclaration { get; }
        public static SymbolDisplayFormat MethodOrPropertyReturnType { get; }
        public static SymbolDisplayFormat ParameterReference { get; }
        public static SymbolDisplayFormat PropertyReference { get; }
        public static SymbolDisplayFormat TypeofParameter { get; }
        public static SymbolDisplayFormat TypeofParameterNoTypeParameters { get; }
        public static SymbolDisplayFormat TypofParameterNoTypeParametersNoQualificationNoEscape { get; }
        public static SymbolDisplayFormat TypeParameter { get; }

        static SymbolDisplayFormats()
        {
            SymbolName = new SymbolDisplayFormat(
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            Namespace = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            ClassName = new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            QualifiedClassNameWithTypeConstraints = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
                    | SymbolDisplayGenericsOptions.IncludeTypeConstraints,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                    | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

            ConstructorName = new SymbolDisplayFormat();

            ImplementedInterface = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                    | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

            PropertyDeclaration = new SymbolDisplayFormat(
                propertyStyle: SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            ImplicitMethodDeclaration = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
                    | SymbolDisplayGenericsOptions.IncludeTypeConstraints,
                memberOptions: SymbolDisplayMemberOptions.IncludeParameters
                    | SymbolDisplayMemberOptions.IncludeConstantValue
                    | SymbolDisplayMemberOptions.IncludeRef,
                parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut
                    | SymbolDisplayParameterOptions.IncludeType
                    | SymbolDisplayParameterOptions.IncludeName
                    | SymbolDisplayParameterOptions.IncludeDefaultValue,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                    | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

            ExplicitMethodDeclaration = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions: SymbolDisplayMemberOptions.IncludeParameters
                    | SymbolDisplayMemberOptions.IncludeConstantValue
                    | SymbolDisplayMemberOptions.IncludeRef,
                parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut
                    | SymbolDisplayParameterOptions.IncludeType
                    | SymbolDisplayParameterOptions.IncludeName,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                    | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

            MethodOrPropertyReturnType = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                    | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

            ParameterReference = new SymbolDisplayFormat(
                parameterOptions: SymbolDisplayParameterOptions.IncludeName,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            PropertyReference = new SymbolDisplayFormat(
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            TypeofParameter = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                    | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

            TypeofParameterNoTypeParameters = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            TypofParameterNoTypeParametersNoQualificationNoEscape = new SymbolDisplayFormat();

            TypeParameter = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                    | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
        }
    }
}
