using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using RestEase.Implementation.Analysis;

namespace RestEase.Implementation.Emission
{
    internal partial class DiagnosticReporter
    {
        private readonly TypeModel typeModel;
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public DiagnosticReporter(TypeModel typeModel)
        {
            this.typeModel = typeModel;
        }

        public void ReportHeaderOnInterfaceMustHaveValue(AttributeModel<HeaderAttribute> header)
        {
            throw new NotImplementedException();
        }

        public void ReportHeaderOnInterfaceMustNotHaveColonInName(AttributeModel<HeaderAttribute> header)
        {
            throw new NotImplementedException();
        }

        public void ReportAllowAnyStatisCodeAttributeNotAllowedOnParentInterface(AllowAnyStatusCodeAttributeModel attribute)
        {
            throw new NotImplementedException();
        }

        public void ReportEventNotAllowed(EventModel _)
        {
            throw new NotImplementedException();
        }

        public void ReportRequesterPropertyMustHaveZeroAttributes(PropertyModel property, List<AttributeModel> attributes)
        {
            throw new NotImplementedException();
        }

        public void ReportPropertyMustHaveOneAttribute(PropertyModel property)
        {
            throw new NotImplementedException();
        }

        public void ReportPropertyMustBeReadOnly(PropertyModel property)
        {
            throw new NotImplementedException();
        }

        public void ReportPropertyMustBeReadWrite(PropertyModel property)
        {
            throw new NotImplementedException();
        }

        public void ReportMultipleRequesterPropertiesNotAllowed(PropertyModel property)
        {
            throw new NotImplementedException();
        }

        public void ReportMissingPathPropertyForBasePathPlaceholder(string missingParam, string basePath)
        {
            throw new NotImplementedException();
        }

        public void ReportMethodMustHaveRequestAttribute(MethodModel method)
        {
            throw new NotImplementedException();
        }

        public void ReportMultiplePathPropertiesForKey(string key, IEnumerable<PropertyModel> _)
        {
            throw new NotImplementedException();
        }

        public void ReportMultiplePathParametersForKey(MethodModel method, string key, IEnumerable<ParameterModel> _)
        {
            throw new NotImplementedException();
        }

        public void ReportHeaderPropertyWithValueMustBeNullable(PropertyModel property)
        {
            throw new NotImplementedException();
        }

        public void ReportHeaderPropertyNameMustContainColon(PropertyModel property)
        {
            throw new NotImplementedException();
        }

        public void ReportMissingPathPropertyOrParameterForPlaceholder(MethodModel method, string placeholder)
        {
            throw new NotImplementedException();
        }

        public void ReportMissingPlaceholderForPathParameter(MethodModel method, string placeholder)
        {
            throw new NotImplementedException();
        }

        public void ReportDuplicateHttpRequestMessagePropertyKey(MethodModel method, string key, IEnumerable<ParameterModel> _)
        {
            throw new NotImplementedException();
        }

        public void ReportParameterMustHaveZeroOrOneAttributes(MethodModel method, ParameterModel parameter, List<AttributeModel> _)
        {
            throw new NotImplementedException();
        }

        private static readonly DiagnosticDescriptor multipleCancellationTokenParameters = CreateDescriptor(
            DiagnosticCode.MultipleCancellationTokenParameters,
            "Methods must not have multiple CancellationToken parameters",
            "Method '{0}': only a single CancellationToken parameter is allowed, found a duplicate parameter '{1}'");
        public void ReportMultipleCancellationTokenParameters(MethodModel method, ParameterModel parameter)
        {
            // If we can (and I'm not sure why we might not be able to), get the location of the entire parameter 'CancellationToken foo',
            // not just the the name 'foo'.
            var location = parameter.ParameterSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? parameter.ParameterSymbol.Locations.FirstOrDefault();
            this.AddDiagnostic(multipleCancellationTokenParameters, location, method.MethodSymbol.Name, parameter.ParameterSymbol.Name);
        }

        public void ReportCancellationTokenMustHaveZeroAttributes(MethodModel method, ParameterModel parameter)
        {
            throw new NotImplementedException();
        }

        public void ReportHeaderOnMethodMustNotHaveColonInName(MethodModel method, AttributeModel<HeaderAttribute> header)
        {
            throw new NotImplementedException();
        }

        public void ReportMultipleBodyParameters(MethodModel method, ParameterModel _)
        {
            throw new NotImplementedException();
        }

        public void ReportQueryMapParameterIsNotADictionary(MethodModel method, ParameterModel _)
        {
            throw new NotImplementedException();
        }

        public void ReportHeaderParameterMustNotHaveValue(MethodModel method, ParameterModel parameter)
        {
            throw new NotImplementedException();
        }

        public void ReportHeaderParameterMustNotHaveColonInName(MethodModel method, ParameterModel parameter)
        {
            throw new NotImplementedException();
        }

        public void ReportMethodMustHaveValidReturnType(MethodModel method)
        {
            throw new NotImplementedException();
        }

        private static DiagnosticDescriptor CreateDescriptor(DiagnosticCode code, string title, string messageFormat) =>
            new DiagnosticDescriptor(code.Format(), title, messageFormat, "RestEaseGeneration", DiagnosticSeverity.Error, isEnabledByDefault: true);


        private void AddDiagnostic(DiagnosticDescriptor descriptor, ImmutableArray<Location> locations, params object?[] args) =>
            this.AddDiagnostic(descriptor, locations.FirstOrDefault(), args);

        private void AddDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[] args)
        {
            this.Diagnostics.Add(Diagnostic.Create(descriptor, location ?? Location.None, args));
        }
    }
}