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
        public static SymbolDisplayFormat TypeNameWithConstraints { get; }

        /// <summary>
        /// The fully-qualified name of a class, including type parameters and constraints, suitably escaped.
        /// E.g. "global::My.Namespace.Foo&lt;T&gt; where T : global::System.Collections.Generic.IEnumerable&lt;T&gt;"
        /// </summary>
        public static SymbolDisplayFormat QualifiedTypeNameWithTypeConstraints { get; }

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
        /// A string suitable for use when declaring a method which implicitly implements an interface method,
        /// suitably escaped. Doesn't include return type or accessibility, but includes parameters, type constraints,
        /// default values. 
        /// E.g. "global::Some.Foo&lt;T&gt;(T a, global::System.Collections.Generic.List&lt;T&gt; b = null) where T : global::System.IEquatable&lt;T&gt;"
        /// </summary>
        /// <remarks>
        /// This has to include IncludeContainingType otherwise enum default values don't get the enum type added
        /// (e.g. Foo(global::SomeEnum e = SomeEnumMember), as the formatter treats the enum member access as a field
        /// access and doesn't put its containing type in otherwise. This does mean we'll have to strip off the containing
        /// type of the method however...
        /// </remarks>
        public static SymbolDisplayFormat ImplicitMethodDeclaration { get; }

        /// <summary>
        /// A string suitable for use when declaring a method which explicitly implements an interface method,
        /// suitably escaped. Includes return type and params. Doesn't include type constraints or default
        /// values, as these shouldn't be given on explicit implementations. E.g.
        /// "Foo&lt;T&gt;(T a, global::System.Collections.Generic.List&lt;T&gt; b)
        /// </summary>
        public static SymbolDisplayFormat ExplicitMethodDeclaration { get; }

        /// <summary>
        /// A string suitable for use as the return type from a property or method, suitably escaped.
        /// E.g. "global::System.Collections.Generic.IEnumerable&lt;T&gt;"
        /// </summary>
        public static SymbolDisplayFormat MethodOrPropertyReturnType { get; }

        /// <summary>
        /// A string suitable for referring to a method parameter, suitably escaped. E.g. "arg"
        /// </summary>
        public static SymbolDisplayFormat ParameterReference { get; }

        /// <summary>
        /// A string suitable for referring to the name of a property, suitably escaped. E.g. "Foo"
        /// </summary>
        public static SymbolDisplayFormat PropertyReference { get; }

        /// <summary>
        /// A string suitable for passing to typeof() or default(), suitably escaped.
        /// Includes type parameters. E.g. "global::My.Class&lt;T&gt;"
        /// </summary>
        public static SymbolDisplayFormat TypeofParameter { get; }

        /// <summary>
        /// A string suitable for the base of a typeof() expression for the generic type definition of a type,
        /// suitably escaped. Returns e.g. "global::System.Collections.Generic.List", to which you will need to add
        /// "&lt;&gt;" using <see cref="RoslynEmitUtils.AddBareAngles(INamedTypeSymbol, string)"/>.
        /// </summary>
        public static SymbolDisplayFormat TypeofParameterNoTypeParameters { get; }

        /// <summary>
        /// Similar to <see cref="TypeofParameterNoTypeParameters"/>, but doesn't escape keywords or fully-qualify types,
        /// e.g. "List". Useful if the thing passed to <see cref="RoslynEmitUtils.AddBareAngles(INamedTypeSymbol, string)"/>
        /// needs a prefix adding to it.
        /// </summary>
        public static SymbolDisplayFormat TypofParameterNoTypeParametersNoQualificationNoEscape { get; }

        /// <summary>
        /// A string suitable for passing as a type parameter (i.e. between the &lt; and &gt; when constructing
        /// a call to a generic methods), suitably escaped. E.g. "global::System.Collections.Generic.List&lt;int&gt;"
        /// </summary>
        public static SymbolDisplayFormat TypeParameter { get; }

        /// <summary>
        /// A string suitable for using as part of a generated file name
        /// </summary>
        public static SymbolDisplayFormat GeneratedFileName { get; }

        static SymbolDisplayFormats()
        {
            SymbolName = new SymbolDisplayFormat(
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            Namespace = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            TypeNameWithConstraints = new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            QualifiedTypeNameWithTypeConstraints = new SymbolDisplayFormat(
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
                    | SymbolDisplayMemberOptions.IncludeRef
                    | SymbolDisplayMemberOptions.IncludeContainingType,
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
                    | SymbolDisplayMemberOptions.IncludeRef
                    | SymbolDisplayMemberOptions.IncludeType
                    | SymbolDisplayMemberOptions.IncludeContainingType,
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
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

            TypofParameterNoTypeParametersNoQualificationNoEscape = new SymbolDisplayFormat();

            TypeParameter = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                    | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

            GeneratedFileName = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
        }
    }
}
