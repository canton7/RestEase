using System;
using System.Collections.Generic;
using System.Linq;
using RestEase.Implementation.Analysis;

namespace RestEase.Implementation.Emission
{
    internal partial class DiagnosticReporter
    {
        private readonly TypeModel typeModel;

        public DiagnosticReporter(TypeModel typeModel)
        {
            this.typeModel = typeModel;
        }

        public void ReportHeaderOnInterfaceMustHaveValue(AttributeModel<HeaderAttribute> header)
        {
            throw new ImplementationCreationException($"[Header(\"{header.Attribute.Name}\")] on interface must have the form [Header(\"Name\", \"Value\")]");
        }

        public void ReportHeaderOnInterfaceMustNotHaveColonInName(AttributeModel<HeaderAttribute> header)
        {
            string desc = header.Attribute.Value == null
                ? $"Header(\"{header.Attribute.Name}\")"
                : $"Header(\"{header.Attribute.Name}\", \"{header.Attribute.Value}\")";
            throw new ImplementationCreationException($"[{desc}] on interface must not have a colon in the header name");
        }

        public void ReportAllowAnyStatisCodeAttributeNotAllowedOnParentInterface(AllowAnyStatusCodeAttributeModel attribute)
        {
            throw new ImplementationCreationException($"Parent interface {attribute.ContainingType.Name} may not have any [AllowAnyStatusCode] attributes");
        }

        public void ReportEventNotAllowed(EventModel _)
        {
            throw new ImplementationCreationException("Interfaces must not have any events");
        }

        public void ReportRequesterPropertyMustHaveZeroAttributes(PropertyModel property, List<AttributeModel> attributes)
        {
            throw new ImplementationCreationException($"{nameof(IRequester)} property {property.PropertyInfo.Name} must not have the following attributes: {string.Join(", ", attributes.Select(x => x.AttributeName))}");
        }

        public void ReportPropertyMustHaveOneAttribute(PropertyModel property)
        {
            throw new ImplementationCreationException($"Property {property.PropertyInfo.Name} must have exactly one attribute");
        }

        public void ReportPropertyMustBeReadOnly(PropertyModel property)
        {
            throw new ImplementationCreationException($"Property {property.PropertyInfo.Name} must have a getter but not a setter");
        }

        public void ReportPropertyMustBeReadWrite(PropertyModel property)
        {
            throw new ImplementationCreationException($"Property {property.PropertyInfo.Name} must have a getter and a setter");
        }

        public void ReportMultipleRequesterPropertiesNotAllowed(PropertyModel property)
        {
            throw new ImplementationCreationException($"Property {property.PropertyInfo.Name}: there must not be more than one property of type {nameof(IRequester)}");
        }

        public void ReportMissingPathPropertyForBasePathPlaceholder(string missingParam, string basePath)
        {
            throw new ImplementationCreationException($"Unable to find a [Path(\"{missingParam}\")] property for the path placeholder '{{{missingParam}}}' in [BasePath(\"{basePath}\")]");
        }

        public void ReportMethodMustHaveRequestAttribute(MethodModel method)
        {
            throw new ImplementationCreationException($"Method {method.MethodInfo.Name} does not have a suitable [Get] / [Post] / etc attribute on it");
        }

        public void ReportMultiplePathPropertiesForKey(string key, IEnumerable<PropertyModel> _)
        {
            throw new ImplementationCreationException($"Found more than one path property for key {key}");
        }

        public void ReportMultiplePathParametersForKey(MethodModel method, string key, IEnumerable<ParameterModel> _)
        {
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': Found more than one path property for key {key}");
        }

        public void ReportHeaderPropertyWithValueMustBeNullable(PropertyModel property)
        {
            var headerAttribute = property.HeaderAttribute!.Attribute;
            throw new ImplementationCreationException($"[Header(\"{headerAttribute.Name}\", \"{headerAttribute.Value}\")] on property {property.Name} (i.e. containing a default value) can only be used if the property type is nullable");
        }

        public void ReportHeaderPropertyNameMustContainColon(PropertyModel property)
        {
            var headerAttribute = property.HeaderAttribute!.Attribute;
            throw new ImplementationCreationException($"[Header(\"{headerAttribute.Name}\")] on property {property.Name} must not have a colon in its name");
        }

        public void ReportMissingPathPropertyOrParameterForPlaceholder(MethodModel method, string placeholder)
        {
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': unable to find a [Path(\"{placeholder}\")] property or parameter for the path placeholder '{{{placeholder}}}'");
        }

        public void ReportMissingPlaceholderForPathParameter(MethodModel method, string placeholder)
        {
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': unable to find to find a placeholder {{{placeholder}}} for the path parameter '{placeholder}'");
        }

        public void ReportDuplicateHttpRequestMessagePropertyKey(MethodModel method, string key, IEnumerable<ParameterModel> _)
        {
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': found more than one HTTP request message property for key {key}");
        }

        public void ReportParameterMustHaveZeroOrOneAttributes(MethodModel method, ParameterModel parameter, List<AttributeModel> _)
        {
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': parameter '{parameter.Name}' must have zero or one attributes");
        }

        public void ReportMultipleCancellationTokenParameters(MethodModel method, ParameterModel _)
        {
            throw new ImplementationCreationException(DiagnosticCode.MultipleCancellationTokenParameters, $"Method '{method.MethodInfo.Name}': only a single CancellationToken parameter is allowed");
        }

        public void ReportCancellationTokenMustHaveZeroAttributes(MethodModel method, ParameterModel parameter)
        {
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': CancellationToken parameter '{parameter.Name}' must not have any attributes");
        }

        public void ReportHeaderOnMethodMustNotHaveColonInName(MethodModel method, AttributeModel<HeaderAttribute> header)
        {
            throw new ImplementationCreationException($"[Header(\"{header.Attribute.Name}\")] on method {method.MethodInfo.Name} must not have colon in its name");
        }

        public void ReportMultipleBodyParameters(MethodModel method, ParameterModel _)
        {
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': found more than one parameter with a [Body] attribute");
        }

        public void ReportQueryMapParameterIsNotADictionary(MethodModel method, ParameterModel _)
        {
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': [QueryMap] parameter is not of type IDictionary or IDictionary<TKey, TValue> (or one of their descendents)");
        }

        public void ReportHeaderParameterMustNotHaveValue(MethodModel method, ParameterModel parameter)
        {
            var attribute = parameter.HeaderAttribute!.Attribute;
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': [Header(\"{attribute.Name}\", \"{attribute.Value}\")] must have the form [Header(\"Name\")], not [Header(\"Name\", \"Value\")]");
        }

        public void ReportHeaderParameterMustNotHaveColonInName(MethodModel method, ParameterModel parameter)
        {
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': [Header(\"{parameter.HeaderAttribute!.Attribute.Name}\")] must not have a colon in its name");
        }

        public void ReportMethodMustHaveValidReturnType(MethodModel method)
        {
            throw new ImplementationCreationException($"Method '{method.MethodInfo.Name}': must have a return type of Task<T> or Task");
        }
    }
}