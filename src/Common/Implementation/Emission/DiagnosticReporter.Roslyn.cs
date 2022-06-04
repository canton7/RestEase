using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Tags;
using RestEase.Implementation.Analysis;

namespace RestEase.Implementation.Emission
{
    internal partial class DiagnosticReporter
    {
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();
        public bool HasErrors { get; private set; }

        private static readonly DiagnosticDescriptor typeMustBeAccessible = CreateDescriptor(
            DiagnosticCode.InterfaceTypeMustBeAccessible,
            "Types must be public or internal",
            "Type '{0}' must be public or internal");
        public void ReportTypeMustBeAccessible(TypeModel typeModel)
        {
            this.AddDiagnostic(typeMustBeAccessible, typeModel.NamedTypeSymbol.Locations, typeModel.NamedTypeSymbol.Name);
        }

        private static readonly DiagnosticDescriptor headerMustNotHaveColonInName = CreateDescriptor(
            DiagnosticCode.HeaderMustNotHaveColonInName,
            "Header attributes must not have colons in their names",
            "Header attribute name '{0}' must not contain a colon");
        public void ReportHeaderOnInterfaceMustNotHaveColonInName(TypeModel typeModel, AttributeModel<HeaderAttribute> header)
        {
            this.AddDiagnostic(
                headerMustNotHaveColonInName,
                AttributeLocations(header, typeModel.NamedTypeSymbol),
                header.Attribute!.Name);
        }

        public void ReportPropertyHeaderMustNotHaveColonInName(PropertyModel property)
        {
            this.AddDiagnostic(
                headerMustNotHaveColonInName,
                AttributeLocations(property.HeaderAttribute, property.PropertySymbol),
                property.HeaderAttribute!.Attribute.Name);
        }

        public void ReportHeaderOnMethodMustNotHaveColonInName(MethodModel method, AttributeModel<HeaderAttribute> header)
        {
            this.AddDiagnostic(
                headerMustNotHaveColonInName,
                AttributeLocations(header, method.MethodSymbol),
                header.Attribute!.Name);
        }

        public void ReportHeaderParameterMustNotHaveColonInName(MethodModel _, ParameterModel parameter)
        {
            this.AddDiagnostic(
                headerMustNotHaveColonInName,
                AttributeLocations(parameter.HeaderAttribute, parameter.ParameterSymbol),
                parameter.HeaderAttribute!.Attribute.Name);
        }

        private static readonly DiagnosticDescriptor headerOnInterfaceMustHaveValue = CreateDescriptor(
            DiagnosticCode.HeaderOnInterfaceMustHaveValue,
            "Header attributes on the interface must have a value",
            "Header on interface must have a value (i.e. be of the form [Header(\"{0}\", \"Value Here\")])");
        public void ReportHeaderOnInterfaceMustHaveValue(TypeModel typeModel, AttributeModel<HeaderAttribute> header)
        {
            this.AddDiagnostic(headerOnInterfaceMustHaveValue, AttributeLocations(header, typeModel.NamedTypeSymbol), header.Attribute.Name);
        }

        private static readonly DiagnosticDescriptor allowAnyStatusCodeAttributeNotAllowedOnParentInterface = CreateDescriptor(
            DiagnosticCode.AllowAnyStatusCodeAttributeNotAllowedOnParentInterface,
            "Parent interfaces may not have any [AllowAnyStatusCode] attributes",
            "Parent interface (of type '{0}') may not have an [AllowAnyStatusCode] attribute");
        public void ReportAllowAnyStatusCodeAttributeNotAllowedOnParentInterface(TypeModel typeModel, AttributeModel<AllowAnyStatusCodeAttribute> attribute)
        {
            this.AddDiagnostic(
                allowAnyStatusCodeAttributeNotAllowedOnParentInterface,
                AttributeLocations(attribute, attribute.DeclaringSymbol),
                typeModel.NamedTypeSymbol.Name);
        }

        private static readonly DiagnosticDescriptor eventsNotAllowed = CreateDescriptor(
            DiagnosticCode.EventsNotAllowed,
            "Interfaces must not have any events",
            "Interface must not have any events");
        public void ReportEventNotAllowed(EventModel eventModel)
        {
            this.AddDiagnostic(eventsNotAllowed, eventModel.EventSymbol.Locations);
        }

        private static readonly DiagnosticDescriptor propertyMustHaveOneAttribute = CreateDescriptor(
            DiagnosticCode.PropertyMustHaveOneAttribute,
            "Properties must have exactly one attribute",
            "Property must have exactly one attribute");
        public void ReportPropertyMustHaveOneAttribute(PropertyModel property)
        {
            this.AddDiagnostic(propertyMustHaveOneAttribute, property.PropertySymbol.Locations);
        }

        private static readonly DiagnosticDescriptor propertyMustBeReadOnly = CreateDescriptor(
            DiagnosticCode.PropertyMustBeReadOnly,
            "Property must be read-only",
            "Property must have a getter but not a setter");

        public void ReportPropertyMustBeReadOnly(PropertyModel property)
        {
            // We don't want to include the headers in the squiggle
            this.AddDiagnostic(propertyMustBeReadOnly, property.PropertySymbol.Locations);
        }

        private static readonly DiagnosticDescriptor propertyMustBeReadWrite = CreateDescriptor(
            DiagnosticCode.PropertyMustBeReadWrite,
            "Property must be read/write",
            "Property must have a getter and a setter");

        public void ReportPropertyMustBeReadWrite(PropertyModel property)
        {
            // We don't want to include the headers in the squiggle
            this.AddDiagnostic(propertyMustBeReadWrite, property.PropertySymbol.Locations);
        }

        private static readonly DiagnosticDescriptor requesterPropertyMustHaveZeroAttributes = CreateDescriptor(
            DiagnosticCode.RequesterPropertyMustHaveZeroAttributes,
            "IRequester properties must not have any attributes",
            "IRequester property must not have any attribtues");
        public void ReportRequesterPropertyMustHaveZeroAttributes(PropertyModel propertyModel, List<AttributeModel> attributes)
        {
            this.AddDiagnostic(
                requesterPropertyMustHaveZeroAttributes,
                AttributeLocations(attributes, propertyModel.PropertySymbol));
        }

        private static readonly DiagnosticDescriptor multipleRequesterProperties = CreateDescriptor(
            DiagnosticCode.MultipleRequesterProperties,
            "Only a single IRequester property is allowed",
            "There must not be more than one property of type IRequester");
        public void ReportMultipleRequesterProperties(PropertyModel property)
        {
            this.AddDiagnostic(multipleRequesterProperties, property.PropertySymbol.Locations);
        }

        private static readonly DiagnosticDescriptor baseAddressMustBeAbsolute = CreateDescriptor(
            DiagnosticCode.BaseAddressMustBeAbsolute,
            "BaseAddress must be an absolute URI",
            "Base address '{0}' must be an absolute URI");
        public void ReportBaseAddressMustBeAbsolute(TypeModel typeModel, AttributeModel<BaseAddressAttribute> attributeModel)
        {
            this.AddDiagnostic(baseAddressMustBeAbsolute, AttributeLocations(attributeModel, typeModel.NamedTypeSymbol), attributeModel.Attribute.BaseAddress);
        }

        private static readonly DiagnosticDescriptor missingPathPropertyForBaseAddressPlaceholder = CreateDescriptor(
            DiagnosticCode.MissingPathPropertyForBaseAddressPlaceholder,
            "BaseAddress placeholders must have corresponding path properties",
            "Unable to find a [Path(\"{0}\")] property for the path placeholder '{{{0}}}' in base address '{1}'");
        public void ReportMissingPathPropertyForBaseAddressPlaceholder(TypeModel typeModel, AttributeModel<BaseAddressAttribute> attributeModel, string missingParam)
        {
            this.AddDiagnostic(missingPathPropertyForBaseAddressPlaceholder, AttributeLocations(attributeModel, typeModel.NamedTypeSymbol), missingParam, attributeModel.Attribute.BaseAddress);
        }

        private static readonly DiagnosticDescriptor missingPathPropertyForBasePathPlaceholder = CreateDescriptor(
            DiagnosticCode.MissingPathPropertyForBasePathPlaceholder,
            "BasePath placeholders must have corresponding path properties",
            "Unable to find a [Path(\"{0}\")] property for the path placeholder '{{{0}}}' in base path '{1}'");
        public void ReportMissingPathPropertyForBasePathPlaceholder(TypeModel typeModel, AttributeModel<BasePathAttribute> attributeModel, string missingParam)
        {
            this.AddDiagnostic(missingPathPropertyForBasePathPlaceholder, AttributeLocations(attributeModel, typeModel.NamedTypeSymbol), missingParam, attributeModel.Attribute.BasePath);
        }

        private static readonly DiagnosticDescriptor methodMustHaveRequestAttribute = CreateDescriptor(
            DiagnosticCode.MethodMustHaveRequestAttribute,
            "All methods must have a suitable [Get] / [Post] / etc attribute",
            "Method does not have a suitable [Get] / [Post] / etc attribute");
        public void ReportMethodMustHaveRequestAttribute(MethodModel method)
        {
            this.AddDiagnostic(methodMustHaveRequestAttribute, method.MethodSymbol.Locations);
        }

        private static readonly DiagnosticDescriptor methodMustHaveOneRequestAttribute = CreateDescriptor(
            DiagnosticCode.MethodMustHaveOneRequestAttribute,
            "Methods must only have a single [Get] / [Post] / etc attribute",
            "Method must only have a single request-related attribute, found ({0})");
        public void ReportMethodMustHaveOneRequestAttribute(MethodModel method)
        {
            this.AddDiagnostic(
                methodMustHaveOneRequestAttribute,
                AttributeLocations(method.RequestAttributes, method.MethodSymbol),
                string.Join(", ", method.RequestAttributes.Select(x => Regex.Replace(x.AttributeName, "Attribute$", ""))));
        }

        private static readonly DiagnosticDescriptor multiplePathPropertiesForKey = CreateDescriptor(
            DiagnosticCode.MultiplePathPropertiesForKey,
            "There must not be multiple path properties for the same key",
            "Multiple path properties for the key '{0}' are not allowed");
        public void ReportMultiplePathPropertiesForKey(string key, IEnumerable<PropertyModel> properties)
        {
            this.AddDiagnostic(multiplePathPropertiesForKey, properties.SelectMany(x => SymbolLocations(x.PropertySymbol)), key);
        }

        private static readonly DiagnosticDescriptor multiplePathParametersForKey = CreateDescriptor(
            DiagnosticCode.MultiplePathParametersForKey,
            "There must not be multiple path parameters for the same key",
            "Multiple path parameters for the key '{0}' are not allowed");
        public void ReportMultiplePathParametersForKey(MethodModel _, string key, IEnumerable<ParameterModel> parameters)
        {
            this.AddDiagnostic(multiplePathParametersForKey, parameters.SelectMany(x => SymbolLocations(x.ParameterSymbol)), key);
        }

        private static readonly DiagnosticDescriptor headerPropertyWithValueMustBeNullable = CreateDescriptor(
            DiagnosticCode.HeaderPropertyWithValueMustBeNullable,
            "Property header which has a default value must be of a type which is nullable",
            "[Header(\"{0}\", \"{1}\")] on property (i.e. containing a default value) can only be used if the property type is nullable");
        public void ReportHeaderPropertyWithValueMustBeNullable(PropertyModel property)
        {
            var attribute = property.HeaderAttribute!.Attribute;
            this.AddDiagnostic(
                headerPropertyWithValueMustBeNullable,
                AttributeLocations(property.HeaderAttribute, property.PropertySymbol),
                attribute.Name, attribute.Value);
        }

        private static readonly DiagnosticDescriptor missingPathPropertyOrParameterForPlaceholder = CreateDescriptor(
            DiagnosticCode.MissingPathPropertyOrParameterForPlaceholder,
            "All placeholders in a path must have a corresponding path property or path parameter",
            "No path property or parameter '{0}' found for placeholder {{{0}}}");
        public void ReportMissingPathPropertyOrParameterForPlaceholder(MethodModel method, AttributeModel<RequestAttributeBase> requestAttribute, string placeholder)
        {
            // We'll put the squiggle on the attribute itself
            this.AddDiagnostic(missingPathPropertyOrParameterForPlaceholder, AttributeLocations(requestAttribute, method.MethodSymbol), placeholder);
        }

        private static readonly DiagnosticDescriptor missingPlaceholderForPathParameter = CreateDescriptor(
            DiagnosticCode.MissingPlaceholderForPathParameter,
            "All path parameters on a method must have a corresponding placeholder",
            "No placeholder {{{0}}} found for path parameter '{0}'");
        public void ReportMissingPlaceholderForPathParameter(MethodModel _, string placeholder, IEnumerable<ParameterModel> parameters)
        {
            this.AddDiagnostic(missingPlaceholderForPathParameter, parameters.SelectMany(x => SymbolLocations(x.ParameterSymbol)), placeholder);
        }

        private static readonly DiagnosticDescriptor multipleHttpRequestMessagePropertiesForKey = CreateDescriptor(
            DiagnosticCode.MultipleHttpRequestMessagePropertiesForKey,
            "Multiple properties must not have the same HttpRequestMessageProperty key",
            "Multiple properties found for HttpRequestMessageProperty key '{0}'");
        public void ReportMultipleHttpRequestMessagePropertiesForKey(string key, IEnumerable<PropertyModel> properties)
        {
            this.AddDiagnostic(
                multipleHttpRequestMessagePropertiesForKey,
                properties.SelectMany(x => SymbolLocations(x.PropertySymbol)),
                key);
        }

        private static readonly DiagnosticDescriptor httpRequestMessageParamDuplicatesPropertyForKey = CreateDescriptor(
            DiagnosticCode.HttpRequestMessageParamDuplicatesPropertyForKey,
            "Method parameters must not have the same HttpRequsetMessageProperty key as a property",
            "Method parameter has the same HttpRequestMessageProperty key '{0}' as property '{1}'");
        public void ReportHttpRequestMessageParamDuplicatesPropertyForKey(MethodModel _, string key, PropertyModel property, ParameterModel parameter)
        {
            this.AddDiagnostic(
                httpRequestMessageParamDuplicatesPropertyForKey,
                SymbolLocations(parameter.ParameterSymbol),
                key, property.Name);
        }

        private static readonly DiagnosticDescriptor multipleHttpRequestMessageParametersForKey = CreateDescriptor(
            DiagnosticCode.MultipleHttpRequestMessageParametersForKey,
            "Method parameters must not have the same HttpRequestMessageProperty key",
            "Multiple parameters found for HttpRequestMessageProperty key '{0}'");
        public void ReportMultipleHttpRequestMessageParametersForKey(MethodModel _, string key, IEnumerable<ParameterModel> parameters)
        {
            this.AddDiagnostic(
                multipleHttpRequestMessageParametersForKey,
                parameters.SelectMany(x => SymbolLocations(x.ParameterSymbol)),
                key);
        }

        private static readonly DiagnosticDescriptor parameterMustHaveZeroOrOneAttributes = CreateDescriptor(
            DiagnosticCode.ParameterMustHaveZeroOrOneAttributes,
            "Method parameters must not have zero or one attributes",
            "Method parameter '{0}' has {1} attributes, but it must have zero or one");
        public void ReportParameterMustHaveZeroOrOneAttributes(MethodModel _, ParameterModel parameter, List<AttributeModel> attributes)
        {
            this.AddDiagnostic(
                parameterMustHaveZeroOrOneAttributes,
                SymbolLocations(parameter.ParameterSymbol),
                parameter.Name,
                attributes.Count);
        }

        private static readonly DiagnosticDescriptor parameterMustNotBeByRef = CreateDescriptor(
            DiagnosticCode.ParameterMustNotBeByRef,
            "Method parameters must not not be ref, in or out",
            "Method parameter '{0}' must not be ref, in or out");
        public void ReportParameterMustNotBeByRef(MethodModel _, ParameterModel parameter)
        {
            this.AddDiagnostic(parameterMustNotBeByRef, SymbolLocations(parameter.ParameterSymbol), parameter.Name);
        }

        private static readonly DiagnosticDescriptor multipleCancellationTokenParameters = CreateDescriptor(
            DiagnosticCode.MultipleCancellationTokenParameters,
            "Methods must not have multiple CancellationToken parameters",
            "Multiple CancellationTokens are not allowed");

        public void ReportMultipleCancellationTokenParameters(MethodModel _, IEnumerable<ParameterModel> parameters)
        {
            this.AddDiagnostic(multipleCancellationTokenParameters, parameters.SelectMany(x => SymbolLocations(x.ParameterSymbol)));
        }

        private static readonly DiagnosticDescriptor cancellationTokenMustHaveZeroAttributes = CreateDescriptor(
            DiagnosticCode.CancellationTokenMustHaveZeroAttributes,
            "CancellationToken parameters must have zero attributes",
            "CancellationToken parameter '{0}' must have zero attributes");
        public void ReportCancellationTokenMustHaveZeroAttributes(MethodModel _, ParameterModel parameter)
        {
            this.AddDiagnostic(
                cancellationTokenMustHaveZeroAttributes,
                SymbolLocations(parameter.ParameterSymbol),
                parameter.Name);
        }

        private static readonly DiagnosticDescriptor multipleBodyParameters = CreateDescriptor(
            DiagnosticCode.MultipleBodyParameters,
            "There must not be multiple body parameters",
            "Found more than one parameter with a [Body] attribute");
        public void ReportMultipleBodyParameters(MethodModel _, IEnumerable<ParameterModel> parameters)
        {
            this.AddDiagnostic(multipleBodyParameters, parameters.SelectMany(x => SymbolLocations(x.ParameterSymbol)));
        }

        private static readonly DiagnosticDescriptor queryMapParameterIsNotADictionary = CreateDescriptor(
            DiagnosticCode.QueryMapParameterIsNotADictionary,
            "QueryMap parameters must be dictionaries",
            "[QueryMap] parameter is not of the type IDictionary or IDictionary<TKey, TValue> (or their descendents)");
        public void ReportQueryMapParameterIsNotADictionary(MethodModel _, ParameterModel parameter)
        {
            this.AddDiagnostic(queryMapParameterIsNotADictionary, SymbolLocations(parameter.ParameterSymbol));
        }

        private static readonly DiagnosticDescriptor headerParameterMustNotHaveValue = CreateDescriptor(
            DiagnosticCode.HeaderParameterMustNotHaveValue,
            "Header attributes on parameters must not have values",
            "Header attribute must have the form [Header(\"{0}\")], not [Header(\"{0}\", \"{1}\")]");
        public void ReportHeaderParameterMustNotHaveValue(MethodModel method, ParameterModel parameter)
        {
            var attribute = parameter.HeaderAttribute!.Attribute;
            this.AddDiagnostic(
                headerParameterMustNotHaveValue,
                AttributeLocations(parameter.HeaderAttribute, method.MethodSymbol),
                attribute.Name, attribute.Value);
        }

        private static readonly DiagnosticDescriptor methodMustHaveValidReturnType = CreateDescriptor(
            DiagnosticCode.MethodMustHaveValidReturnType,
            "Methods must have a return type of Task or Task<T>",
            "Method must have a return type of Task or Task<T>");
        public void ReportMethodMustHaveValidReturnType(MethodModel method)
        {
            this.AddDiagnostic(methodMustHaveValidReturnType, method.MethodSymbol.Locations);
        }

        private static readonly DiagnosticDescriptor queryAttributeConflictWithRawQueryString = CreateDescriptor(
            DiagnosticCode.QueryConflictWithRawQueryString,
            "Parameter attribute RawQueryString must not be specified along with Query",
            "Method '{0}': parameter '{1}', [RawQueryString] must not be specified along with [Query]");
        public void ReportQueryAttributeConflictWithRawQueryString(MethodModel method, ParameterModel parameter)
        {
            this.AddDiagnostic(
                queryAttributeConflictWithRawQueryString,
                SymbolLocations(parameter.ParameterSymbol),
                method.MethodSymbol.Name, parameter.Name);
        }

        // -----------------------------------------------------------
        // Not shared with the Emit DiagnosticReporter

        private static readonly DiagnosticDescriptor couldNotFindRestEaseAssembly = CreateDescriptor(
            DiagnosticCode.CouldNotFindRestEaseAssembly,
            "RestEase must be referenced",
            "Please reference the RestEase NuGet package, in addition to RestEase.SourceGenerator");
        public void ReportCouldNotFindRestEaseAssembly()
        {
            this.AddDiagnostic(couldNotFindRestEaseAssembly, Location.None);
        }

        private static readonly DiagnosticDescriptor restEaseVersionTooOld = CreateDescriptor(
            DiagnosticCode.RestEaseVersionTooOld,
            "A suitable version of RestEase must be referenced",
            "This version of RestEase.SourceGenerator needs a version of RestEase >= {0} and < {1} to be referenced, but version " +
            "{2} was found. Please reference a newer version of RestEase (or downgrade RestEase.SourceGenerator)");
        public void ReportRestEaseVersionTooOld(Version restEaseVersion, Version minInclusive, Version maxExclusive)
        {
            this.AddDiagnostic(restEaseVersionTooOld, Location.None, minInclusive, maxExclusive, restEaseVersion);
        }

        private static readonly DiagnosticDescriptor restEaseVersionTooNew = CreateDescriptor(
            DiagnosticCode.RestEaseVersionTooNew,
            "A suitable version of RestEase must be referenced",
            "This version of RestEase.SourceGenerator needs a version of RestEase >= {0} and < {1} to be referenced, but version " +
            "{2} was found. Please upgrade RestEase.SourceGenerator (or reference an older version of RestEase)");
        public void ReportRestEaseVersionTooNew(Version restEaseVersion, Version minInclusive, Version maxExclusive)
        {
            this.AddDiagnostic(restEaseVersionTooNew, Location.None, minInclusive, maxExclusive, restEaseVersion);
        }

        private static readonly DiagnosticDescriptor couldNotFindRestEaseType = CreateDescriptor(
            DiagnosticCode.CouldNotFindRestEaseType,
            "Unable to find RestEase type",
            "Unable to find RestEase type '{0}'. Make sure you are referencing a suitably recent version of RestEase");
        public void ReportCouldNotFindRestEaseType(string metadataName)
        {
            this.AddDiagnostic(couldNotFindRestEaseType, Location.None, metadataName);
        }

        private static readonly DiagnosticDescriptor couldNotFindSystemType = CreateDescriptor(
            DiagnosticCode.CouldNotFindSystemType,
            "Unable to find System type type",
            "Unable to find System type '{0}'. Make sure you are referencing the appropriate assembly");
        public void ReportCouldNotFindSystemType(string metadataName)
        {
            this.AddDiagnostic(couldNotFindSystemType, Location.None, metadataName);
        }

        private static readonly DiagnosticDescriptor expressionsNotAvailable = CreateDescriptor(
            DiagnosticCode.ExpressionsNotAvailable,
            "Unable to find System.Linq.Expressions.Expression",
            "Unable to find System.Linq.Expressions.Expression. IRequestInfo.MethodInfo will be null. Make sure you are referencing System.Linq.Expressions.dll",
            DiagnosticSeverity.Warning);
        public void ReportExpressionsNotAvailable()
        {
            this.AddDiagnostic(expressionsNotAvailable, Location.None);
        }

        private static readonly DiagnosticDescriptor attributeConstructorNotRecognised = CreateDescriptor(
            DiagnosticCode.AttributeConstructorNotRecognised,
            "Attribute constructor not recognised",
            "Constructor for attribute type '{0}' not recongised. This attribute will be ignored. Make sure you're referencing an up-to-date version of RestEase",
            DiagnosticSeverity.Warning);
        public void ReportAttributeConstructorNotRecognised(AttributeData attributeData, ISymbol declaringSymbol)
        {
            this.AddDiagnostic(
                attributeConstructorNotRecognised,
                AttributeLocations(attributeData, declaringSymbol),
                attributeData.AttributeClass?.Name);
        }

        private static readonly DiagnosticDescriptor attributePropertyNotRecognised = CreateDescriptor(
            DiagnosticCode.AttributePropertyNotRecognised,
            "Attribute property not recognised",
            "Property '{0} for attribute type '{1}' not recongised. This property will be ignored. Make sure you're referencing an up-to-date version of RestEase",
            DiagnosticSeverity.Warning);
        public void ReportAttributePropertyNotRecognised(AttributeData attributeData, KeyValuePair<string, TypedConstant> namedArgument, ISymbol declaringSymbol)
        {
            this.AddDiagnostic(
                attributePropertyNotRecognised,
                AttributeLocations(attributeData, declaringSymbol),
                namedArgument.Key,
                attributeData.AttributeClass?.Name);
        }

        private static DiagnosticDescriptor CreateDescriptor(DiagnosticCode code, string title, string messageFormat, DiagnosticSeverity severity = DiagnosticSeverity.Error)
        {
            string[] tags = severity == DiagnosticSeverity.Error ? new[] { WellKnownDiagnosticTags.NotConfigurable } : Array.Empty<string>();
            return new DiagnosticDescriptor(code.Format(), title, messageFormat, "RestEaseGeneration", severity, isEnabledByDefault: true, customTags: tags);
        }

        private void AddDiagnostic(DiagnosticDescriptor descriptor, IEnumerable<Location> locations, params object?[] args)
        {
            var locationsList = (locations as IReadOnlyList<Location>) ?? locations.ToList();
            this.AddDiagnostic(Diagnostic.Create(descriptor, locationsList.Count == 0 ? Location.None : locationsList[0], locationsList.Skip(1), args));
        }

        private void AddDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[] args)
        {
            this.AddDiagnostic(Diagnostic.Create(descriptor, location ?? Location.None, args));
        }

        private void AddDiagnostic(Diagnostic diagnostic)
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                this.HasErrors = true;
            }
            this.Diagnostics.Add(diagnostic);
        }

        public void Clear()
        {
            this.Diagnostics.Clear();
            this.HasErrors = false;
        }

        // Try and get the location of the whole 'Foo foo', and not just 'foo'
        private static IEnumerable<Location> SymbolLocations(ISymbol symbol)
        {
            var declaringReferences = symbol.DeclaringSyntaxReferences;
            return declaringReferences.Length > 0
                ? declaringReferences.Select(x => x.GetSyntax().GetLocation())
                : symbol.Locations;
        }

        private static IEnumerable<Location> AttributeLocations(AttributeModel? attributeModel, ISymbol fallback)
        {
            return AttributeLocations(attributeModel?.AttributeData, fallback);
        }

        private static IEnumerable<Location> AttributeLocations(AttributeData? attributeData, ISymbol fallback)
        {
            // TODO: This squiggles the 'BasePath(...)' bit. Ideally we'd want '[BasePath(...)]' or perhaps just '...'.
            var attributeLocation = attributeData?.ApplicationSyntaxReference?.GetSyntax().GetLocation();
            return attributeLocation != null ? new[] { attributeLocation } : SymbolLocations(fallback);
        }

        private static IEnumerable<Location> AttributeLocations(IEnumerable<AttributeModel> attributeModels, ISymbol fallback)
        {
            var results = new List<Location>();
            bool anyFailed = false;
            foreach (var attributeModel in attributeModels)
            {
                var location = attributeModel?.AttributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation();
                if (location != null)
                {
                    results.Add(location);
                }
                else
                {
                    anyFailed = true;
                    break;
                }
            }

            return anyFailed || results.Count == 0 ? SymbolLocations(fallback) : results;
        }

    }
}
