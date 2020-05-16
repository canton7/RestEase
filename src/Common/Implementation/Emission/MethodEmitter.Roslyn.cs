using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using RestEase.Implementation.Analysis;

namespace RestEase.Implementation.Emission
{
    internal class MethodEmitter
    {
        private readonly MethodModel methodModel;

        public MethodEmitter(MethodModel methodModel)
        {
            this.methodModel = methodModel;
        }

        public void EmitRequestInfoCreation(RequestAttribute requestAttribute)
        {
            throw new NotImplementedException();
        }

        public void EmitSetCancellationToken(ParameterModel parameter)
        {
            throw new NotImplementedException();
        }

        public void EmitSetAllowAnyStatusCode()
        {
            throw new NotImplementedException();
        }

        public void EmitSetBasePath(string basePath)
        {
            throw new NotImplementedException();
        }

        public void EmitAddMethodHeader(AttributeModel<HeaderAttribute> header)
        {
            throw new NotImplementedException();
        }

        public void EmitAddHeaderProperty(EmittedProperty property)
        {
            Assert(property.PropertyModel.HeaderAttribute != null);
            var attribute = property.PropertyModel.HeaderAttribute.Attribute;
            throw new NotImplementedException();
        }

        public void EmitAddPathProperty(EmittedProperty property, PathSerializationMethod serializationMethod)
        {
            Assert(property.PropertyModel.PathAttribute != null);
            var attribute = property.PropertyModel.PathAttribute.Attribute;
            throw new NotImplementedException();
        }

        public void EmitAddQueryProperty(EmittedProperty property, QuerySerializationMethod serializationMethod)
        {
            Assert(property.PropertyModel.QueryAttribute != null);
            var attribute = property.PropertyModel.QueryAttribute.Attribute;
            throw new NotImplementedException();
        }

        public void EmitAddHttpRequestMessagePropertyProperty(EmittedProperty property)
        {
            throw new NotImplementedException();
        }

        public void EmitSetBodyParameter(ParameterModel parameter, BodySerializationMethod serializationMethod)
        {
            throw new NotImplementedException();
        }

        public bool TryEmitAddQueryMapParameter(ParameterModel parameter, QuerySerializationMethod serializationMethod)
        {
            throw new NotImplementedException();
        }

        public void EmitAddHeaderParameter(ParameterModel parameter)
        {
            Assert(parameter.HeaderAttribute != null);
            var header = parameter.HeaderAttribute.Attribute;
            throw new NotImplementedException();
        }

        public void EmitAddPathParameter(ParameterModel parameter, PathSerializationMethod serializationMethod)
        {
            Assert(parameter.PathAttribute != null);
            var pathParameter = parameter.PathAttribute.Attribute;
            throw new NotImplementedException();
        }

        public void EmitAddQueryParameter(ParameterModel parameter, QuerySerializationMethod serializationMethod)
        {
            // The attribute might be null, if it's a plain parameter
            string? name = parameter.QueryAttribute == null ? parameter.Name : parameter.QueryAttributeName;
            throw new NotImplementedException();
        }

        public void EmitAddHttpRequestMessagePropertyParameter(ParameterModel parameter)
        {
            throw new NotImplementedException();
        }

        public void EmitAddRawQueryStringParameter(ParameterModel parameter)
        {
            throw new NotImplementedException();
        }

        public bool TryEmitRequestMethodInvocation()
        {
            // Call the appropriate RequestVoidAsync/RequestAsync method, depending on whether or not we have a return type
            throw new NotImplementedException();
        }

        [Conditional("DEBUG")]
        private static void Assert([DoesNotReturnIf(false)] bool condition)
        {
            Debug.Assert(condition);
        }
    }
}