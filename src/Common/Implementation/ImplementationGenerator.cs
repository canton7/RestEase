using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RestEase.Implementation.Analysis;
using RestEase.Implementation.Emission;

namespace RestEase.Implementation
{
    internal class ImplementationGenerator
    {
        private static readonly Regex pathParamMatch = new(@"\{(.+?)\}");

        private readonly TypeModel typeModel;
        private readonly Emitter emitter;
        private readonly DiagnosticReporter diagnostics;

        public ImplementationGenerator(TypeModel typeModel, Emitter emitter, DiagnosticReporter diagnosticReporter)
        {
            this.typeModel = typeModel;
            this.emitter = emitter;
            this.diagnostics = diagnosticReporter;
        }

        public EmittedType Generate()
        {
            if (!this.typeModel.IsAccessible)
            {
                this.diagnostics.ReportTypeMustBeAccessible(this.typeModel);
            }

            foreach (var header in this.typeModel.HeaderAttributes)
            {
                if (header.Attribute.Value == null)
                {
                    this.diagnostics.ReportHeaderOnInterfaceMustHaveValue(this.typeModel, header);
                }
                if (header.Attribute.Name.Contains(":"))
                {
                    this.diagnostics.ReportHeaderOnInterfaceMustNotHaveColonInName(this.typeModel, header);
                }
            }

            foreach (var attribute in this.typeModel.AllowAnyStatusCodeAttributes)
            {
                if (!attribute.IsDeclaredOn(this.typeModel))
                {
                    this.diagnostics.ReportAllowAnyStatusCodeAttributeNotAllowedOnParentInterface(this.typeModel, attribute);
                }
            }

            foreach (var @event in this.typeModel.Events)
            {
                this.diagnostics.ReportEventNotAllowed(@event);
            }

            var typeEmitter = this.emitter.EmitType(this.typeModel);

            this.ValidateBaseAddress();
            this.ValidatePathProperties();
            this.ValidateHttpRequestMessageProperties();
            var emittedProperties = this.GenerateProperties(typeEmitter);
            this.GenerateMethods(typeEmitter, emittedProperties);
            return typeEmitter.Generate();
        }

        private List<EmittedProperty> GenerateProperties(TypeEmitter typeEmitter)
        {
            var emittedProperties = new List<EmittedProperty>(this.typeModel.Properties.Count);

            var signatureGrouping = this.typeModel.Properties.GroupBy(x => x.Name);
            foreach (var propertiesWithSignature in signatureGrouping)
            {
                if (propertiesWithSignature.Count() > 1)
                {
                    foreach (var property in propertiesWithSignature)
                    {
                        property.IsExplicit = !property.IsDeclaredOn(this.typeModel);
                    }
                }
            }

            bool hasRequester = false;

            foreach (var property in this.typeModel.Properties)
            {
                var attributes = property.GetAllSetAttributes().ToList();
                if (property.IsRequester)
                {
                    if (hasRequester)
                    {
                        this.diagnostics.ReportMultipleRequesterProperties(property);
                    }

                    if (attributes.Count > 0)
                    {
                        this.diagnostics.ReportRequesterPropertyMustHaveZeroAttributes(property, attributes);
                    }

                    if (!property.HasGetter || property.HasSetter)
                    {
                        this.diagnostics.ReportPropertyMustBeReadOnly(property);
                    }

                    typeEmitter.EmitRequesterProperty(property);

                    hasRequester = true;
                }
                else
                {
                    if (attributes.Count != 1)
                    {
                        this.diagnostics.ReportPropertyMustHaveOneAttribute(property);
                    }
                    else if (!property.HasGetter || !property.HasSetter)
                    {
                        this.diagnostics.ReportPropertyMustBeReadWrite(property);
                    }

                    if (property.HeaderAttribute != null)
                    {
                        var headerAttribute = property.HeaderAttribute.Attribute;
                        if (headerAttribute.Value != null && !property.IsNullable)
                        {
                            this.diagnostics.ReportHeaderPropertyWithValueMustBeNullable(property);
                        }
                        if (headerAttribute.Name.Contains(":"))
                        {
                            this.diagnostics.ReportPropertyHeaderMustNotHaveColonInName(property);
                        }
                    }

                    emittedProperties.Add(typeEmitter.EmitProperty(property));
                }
            }

            return emittedProperties;
        }

        private void GenerateMethods(TypeEmitter typeEmitter, List<EmittedProperty> emittedProperties)
        {
            var signatureGrouping = this.typeModel.Methods.GroupBy(x => x, MethodSignatureEqualityComparer.Instance);
            foreach (var methodsWithSignature in signatureGrouping)
            {
                if (methodsWithSignature.Count() > 1)
                {
                    foreach (var method in methodsWithSignature)
                    {
                        method.IsExplicit = !method.IsDeclaredOn(this.typeModel);
                    }
                }
            }

            foreach (var method in this.typeModel.Methods)
            {
                if (method.IsDisposeMethod)
                {
                    typeEmitter.EmitDisposeMethod(method);
                }
                else
                {
                    this.GenerateMethod(typeEmitter, emittedProperties, method);
                }
            }
        }

        private void GenerateMethod(TypeEmitter typeEmitter, List<EmittedProperty> emittedProperties, MethodModel method)
        {
            var methodEmitter = typeEmitter.EmitMethod(method);
            var serializationMethods = new ResolvedSerializationMethods(this.typeModel.SerializationMethodsAttribute?.Attribute, method.SerializationMethodsAttribute?.Attribute);
            if (method.RequestAttributes.Count == 0)
            {
                this.diagnostics.ReportMethodMustHaveRequestAttribute(method);
            }
            else if (method.RequestAttributes.Count > 1)
            {
                this.diagnostics.ReportMethodMustHaveOneRequestAttribute(method);
            }
            else
            {
                var requestAttribute = method.RequestAttributes[0];
                this.ValidatePathParams(method, requestAttribute);
                this.ValidateCancellationTokenParams(method);
                this.ValidateMultipleBodyParams(method);
                this.ValidateHttpRequestMessageParams(method);

                methodEmitter.EmitRequestInfoCreation(requestAttribute.Attribute);

                var resolvedAllowAnyStatusCode = method.AllowAnyStatusCodeAttribute ?? this.typeModel.TypeAllowAnyStatusCodeAttribute;
                if (resolvedAllowAnyStatusCode?.Attribute.AllowAnyStatusCode == true)
                {
                    methodEmitter.EmitSetAllowAnyStatusCode();
                }

                if (this.typeModel.BaseAddressAttribute?.Attribute.BaseAddress != null)
                {
                    methodEmitter.EmitSetBaseAddress(this.typeModel.BaseAddressAttribute.Attribute.BaseAddress);
                }

                if (this.typeModel.BasePathAttribute?.Attribute.BasePath != null)
                {
                    methodEmitter.EmitSetBasePath(this.typeModel.BasePathAttribute.Attribute.BasePath);
                }

                GenerateMethodProperties(methodEmitter, emittedProperties, serializationMethods);

                foreach (var methodHeader in method.HeaderAttributes)
                {
                    if (methodHeader.Attribute.Name.Contains(":"))
                    {
                        this.diagnostics.ReportHeaderOnMethodMustNotHaveColonInName(method, methodHeader);
                    }

                    methodEmitter.EmitAddMethodHeader(methodHeader);
                }

                this.GenerateMethodParameters(methodEmitter, method, serializationMethods);

                if (!methodEmitter.TryEmitRequestMethodInvocation())
                {
                    this.diagnostics.ReportMethodMustHaveValidReturnType(method);
                }
            }
        }

        private static void GenerateMethodProperties(MethodEmitter methodEmitter, List<EmittedProperty> emittedProperties, ResolvedSerializationMethods serializationMethods)
        {
            foreach (var property in emittedProperties)
            {
                // We've already validated these
                if (property.PropertyModel.HeaderAttribute != null)
                {
                    methodEmitter.EmitAddHeaderProperty(property);
                }
                else if (property.PropertyModel.PathAttribute != null)
                {
                    methodEmitter.EmitAddPathProperty(
                        property,
                        serializationMethods.ResolvePath(property.PropertyModel.PathAttribute.Attribute.SerializationMethod));
                }
                else if (property.PropertyModel.QueryAttribute != null)
                {
                    methodEmitter.EmitAddQueryProperty(
                        property,
                        serializationMethods.ResolveQuery(property.PropertyModel.QueryAttribute.Attribute.SerializationMethod));
                }
                else if (property.PropertyModel.HttpRequestMessagePropertyAttribute != null)
                {
                    methodEmitter.EmitAddHttpRequestMessagePropertyProperty(property);
                }
            }
        }

        private void GenerateMethodParameters(MethodEmitter methodEmitter, MethodModel method, ResolvedSerializationMethods serializationMethods)
        {
            foreach (var parameter in method.Parameters)
            {
                var attributes = parameter.GetAllSetAttributes().ToList();

                if (parameter.IsByRef)
                {
                    this.diagnostics.ReportParameterMustNotBeByRef(method, parameter);
                }

                if (parameter.IsCancellationToken)
                {
                    if (attributes.Count > 0)
                    {
                        this.diagnostics.ReportCancellationTokenMustHaveZeroAttributes(method, parameter);
                    }

                    methodEmitter.EmitSetCancellationToken(parameter);
                }
                else
                {
                    this.ValidateParameterAttributeCombinations(method, parameter);

                    if (parameter.HeaderAttribute != null)
                    {
                        if (parameter.HeaderAttribute.Attribute.Value != null)
                        {
                            this.diagnostics.ReportHeaderParameterMustNotHaveValue(method, parameter);
                        }
                        if (parameter.HeaderAttribute.Attribute.Name.Contains(":"))
                        {
                            this.diagnostics.ReportHeaderParameterMustNotHaveColonInName(method, parameter);
                        }

                        methodEmitter.EmitAddHeaderParameter(parameter);
                    }

                    if (parameter.PathAttribute != null)
                    {
                        methodEmitter.EmitAddPathParameter(
                            parameter,
                            serializationMethods.ResolvePath(parameter.PathAttribute.Attribute.SerializationMethod));
                    }

                    if (parameter.QueryAttribute != null)
                    {
                        methodEmitter.EmitAddQueryParameter(
                            parameter,
                            serializationMethods.ResolveQuery(parameter.QueryAttribute.Attribute.SerializationMethod));
                    }

                    if (parameter.HttpRequestMessagePropertyAttribute != null)
                    {
                        methodEmitter.EmitAddHttpRequestMessagePropertyParameter(parameter);
                    }

                    if (parameter.RawQueryStringAttribute != null)
                    {
                        methodEmitter.EmitAddRawQueryStringParameter(parameter);
                    }

                    if (parameter.QueryMapAttribute != null)
                    {
                        if (!methodEmitter.TryEmitAddQueryMapParameter(parameter, serializationMethods.ResolveQuery(parameter.QueryMapAttribute.Attribute.SerializationMethod)))
                        {
                            this.diagnostics.ReportQueryMapParameterIsNotADictionary(method, parameter);
                        }
                    }

                    if (parameter.BodyAttribute != null)
                    {
                        methodEmitter.EmitSetBodyParameter(
                            parameter,
                            serializationMethods.ResolveBody(parameter.BodyAttribute.Attribute.SerializationMethod));
                    }

                    if (attributes.Count == 0)
                    {
                        methodEmitter.EmitAddQueryParameter(parameter, serializationMethods.ResolveQuery(QuerySerializationMethod.Default));
                    }
                }
            }
        }

        private void ValidateBaseAddress()
        {
            var baseAddress = this.typeModel.BaseAddressAttribute;
            if (baseAddress == null)
            {
                return;
            }

            Uri uri;
            try
            {
                // We expect an absolute URI
                uri = new Uri(baseAddress.Attribute.BaseAddress, UriKind.RelativeOrAbsolute);
            }
            catch
            {
                // If there's a problem parsing it, leave that to fail at runtime.
                return;
            }

            if (!uri.IsAbsoluteUri)
            {
                this.diagnostics.ReportBaseAddressMustBeAbsolute(this.typeModel, baseAddress);
            }
        }

        private void ValidatePathProperties()
        {
            var pathProperties = this.typeModel.Properties.Where(x => x.PathAttribute != null);

            // Check that there are no duplicate param names in the properties
            var duplicateProperties = pathProperties.GroupBy(x => x.PathAttributeName!).Where(x => x.Count() > 1);
            foreach (var properties in duplicateProperties)
            {
                this.diagnostics.ReportMultiplePathPropertiesForKey(properties.Key, properties);
            }

            string? baseAddress = this.typeModel.BaseAddressAttribute?.Attribute.BaseAddress;
            if (baseAddress != null)
            {
                foreach (string missingParam in GetMissingParams(baseAddress))
                {
                    this.diagnostics.ReportMissingPathPropertyForBaseAddressPlaceholder(this.typeModel, this.typeModel.BaseAddressAttribute!, missingParam);
                }
            }

            string? basePath = this.typeModel.BasePathAttribute?.Attribute.BasePath;
            if (basePath != null)
            {
                foreach (string missingParam in GetMissingParams(basePath))
                {
                    this.diagnostics.ReportMissingPathPropertyForBasePathPlaceholder(this.typeModel, this.typeModel.BasePathAttribute!, missingParam);
                }
            }

            IEnumerable<string> GetMissingParams(string path)
            {
                // Check that each placeholder in the base path has a matching path property, and vice versa.
                // We don't consider path parameters here.
                var placeholders = pathParamMatch.Matches(path).Cast<Match>().Select(x => x.Groups[1].Value);

                var missingParams = placeholders.Except(pathProperties.Select(x => x.PathAttributeName!));
                return missingParams.Distinct();
            }
        }

        private void ValidatePathParams(MethodModel method, AttributeModel<RequestAttributeBase> requestAttribute)
        {
            string? path = requestAttribute.Attribute.Path;
            if (path == null)
                path = string.Empty;

            var pathParams = method.Parameters.Where(x => x.PathAttribute != null).ToList();

            // Check that there are no duplicate param names in the attributes
            var duplicateParams = pathParams.GroupBy(x => x.PathAttributeName!).Where(x => x.Count() > 1);
            foreach (var @params in duplicateParams)
            {
                this.diagnostics.ReportMultiplePathParametersForKey(method, @params.Key, @params);
            }

            // Check that each placeholder has a matching attribute, and vice versa
            // We allow a property param to fill in for a missing path param, but we allow them to duplicate
            // each other (the path param takes precedence), and allow a property param which doesn't have a placeholder.
            var placeholders = pathParamMatch.Matches(path).Cast<Match>().Select(x => x.Groups[1].Value).ToList();

            var missingParams = placeholders
                .Except(pathParams.Select(x => x.PathAttributeName!).Concat(this.typeModel.Properties.Select(x => x.PathAttributeName!)));
            foreach (string missingParam in missingParams)
            {
                this.diagnostics.ReportMissingPathPropertyOrParameterForPlaceholder(method, requestAttribute, missingParam);
            }

            var missingPlaceholders = pathParams.Select(x => x.PathAttributeName!).Except(placeholders);
            foreach (string missingPlaceholder in missingPlaceholders)
            {
                var parameters = pathParams.Where(x => x.PathAttributeName == missingPlaceholder);
                this.diagnostics.ReportMissingPlaceholderForPathParameter(method, missingPlaceholder, parameters);
            }
        }

        private void ValidateCancellationTokenParams(MethodModel method)
        {
            var cancellationTokenParams = method.Parameters.Where(x => x.IsCancellationToken).ToList();
            if (cancellationTokenParams.Count > 1)
            {
                this.diagnostics.ReportMultipleCancellationTokenParameters(method, cancellationTokenParams);
            }
        }

        private void ValidateMultipleBodyParams(MethodModel method)
        {
            var bodyParams = method.Parameters.Where(x => x.BodyAttribute != null).ToList();
            if (bodyParams.Count > 1)
            {
                this.diagnostics.ReportMultipleBodyParameters(method, bodyParams);
            }
        }

        private void ValidateHttpRequestMessageProperties()
        {
            var requestProperties = this.typeModel.Properties.Where(x => x.HttpRequestMessagePropertyAttribute != null);
            var duplicateProperties = requestProperties
                .GroupBy(x => x.HttpRequestMessagePropertyAttributeKey!)
                .Where(x => x.Count() > 1);
            foreach (var properties in duplicateProperties)
            {
                this.diagnostics.ReportMultipleHttpRequestMessagePropertiesForKey(properties.Key, properties);
            }
        }

        private void ValidateHttpRequestMessageParams(MethodModel method)
        {
            // Check that there are no duplicate param names in the attributes
            var requestParams = method.Parameters.Where(x => x.HttpRequestMessagePropertyAttribute != null).ToList();

            foreach (var requestParam in requestParams)
            {
                var duplicateProperty = this.typeModel.Properties
                    .FirstOrDefault(x => x.HttpRequestMessagePropertyAttributeKey == requestParam.HttpRequestMessagePropertyAttributeKey);
                if (duplicateProperty != null)
                {
                    this.diagnostics.ReportHttpRequestMessageParamDuplicatesPropertyForKey(
                        method,
                        requestParam.HttpRequestMessagePropertyAttributeKey!,
                        duplicateProperty,
                        requestParam);
                }
            }

            var duplicateParams = requestParams
                .GroupBy(x => x.HttpRequestMessagePropertyAttributeKey!)
                .Where(x => x.Count() > 1);
            foreach (var @params in duplicateParams)
            {
                this.diagnostics.ReportMultipleHttpRequestMessageParametersForKey(method, @params.Key, @params);
            }
        }

        private void ValidateParameterAttributeCombinations(MethodModel method, ParameterModel parameter)
        {
            if (parameter.QueryAttribute != null && parameter.RawQueryStringAttribute != null)
            {
                this.diagnostics.ReportQueryAttributeConflictWithRawQueryString(method, parameter);
            }
        }
    }
}
