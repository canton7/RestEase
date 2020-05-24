using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RestEase.Implementation.Analysis;
using RestEase.SourceGenerator;
using RestEase.SourceGenerator.Implementation;
using static RestEase.SourceGenerator.Implementation.EmitUtils;

namespace RestEase.Implementation.Emission
{
    internal class MethodEmitter
    {
        private readonly MethodModel methodModel;
        private readonly IndentedTextWriter writer;
        private readonly WellKnownSymbols wellKnownSymbols;
        private readonly string qualifiedTypeName;
        private readonly string requesterFieldName;
        private readonly string? classHeadersFieldName;
        private readonly int index;
        private readonly string requestInfoLocalName;

        public MethodEmitter(
            MethodModel methodModel,
            IndentedTextWriter writer,
            WellKnownSymbols wellKnownSymbols,
            string qualifiedTypeName,
            string requesterFieldName,
            string? classHeadersFieldName,
            int index)
        {
            this.methodModel = methodModel;
            this.writer = writer;
            this.wellKnownSymbols = wellKnownSymbols;
            this.qualifiedTypeName = qualifiedTypeName;
            this.requesterFieldName = requesterFieldName;
            this.classHeadersFieldName = classHeadersFieldName;
            this.index = index;
            this.requestInfoLocalName = this.GenerateRequestInfoLocalName();

            this.EmitMethodInfoField();
            this.EmitMethodDeclaration();
        }

        private string GenerateRequestInfoLocalName()
        {
            // We need to pick a name for the requestInfo that they haven't picked for a parameter
            string name = "requestInfo";
            if (this.methodModel.MethodSymbol.Parameters.Any(x => x.Name == name))
            {
                int i = 0;
                do
                {
                    name = "requestInfo" + i;
                    i++;
                } while (this.methodModel.MethodSymbol.Parameters.Any(x => x.Name == name));
            }
            return name;
        }

        private void EmitMethodInfoField()
        {
            this.writer.WriteLine("private static global::System.Reflection.MethodInfo methodInfo_" + this.index + ";");
        }

        private void EmitMethodDeclaration()
        {
            // The MethodSymbol represents the interface method, not the implementation, so we can't get ToDisplayString
            // to give us the explicit interface implementation bit
            this.writer.Write(this.methodModel.MethodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormats.MethodReturnType));
            this.writer.Write(" ");
            this.writer.Write(this.methodModel.MethodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormats.ImplementedInterface));
            this.writer.Write(".");
            this.writer.WriteLine(this.methodModel.MethodSymbol.ToDisplayString(SymbolDisplayFormats.MethodDeclaration));
            this.writer.WriteLine("{");
            this.writer.Indent++;
        }

        public void EmitRequestInfoCreation(RequestAttribute requestAttribute)
        {
            this.writer.Write("var " + this.requestInfoLocalName + " = new " + WellKnownNames.RequestInfo + "(");
            if (WellKnownNames.HttpMethodProperties.TryGetValue(requestAttribute.Method, out string httpMethod))
            {
                this.writer.Write(httpMethod);
            }
            else
            {
                this.writer.Write("new global::System.Net.Http.HttpMethod(" + requestAttribute.Method.Method + ")");
            }
            this.writer.Write(", " + QuoteString(requestAttribute.Path ?? string.Empty));
            this.writer.Write(", " + this.qualifiedTypeName + "." + "methodInfo_" + this.index);
            this.writer.WriteLine(");");

            if (this.classHeadersFieldName != null)
            {
                this.writer.WriteLine(this.requestInfoLocalName + ".ClassHeaders = " + this.qualifiedTypeName + "." +
                    this.classHeadersFieldName + ";");
            }
        }

        public void EmitSetCancellationToken(ParameterModel parameter)
        {
            this.writer.WriteLine(this.requestInfoLocalName + ".CancellationToken = " + ReferenceTo(parameter) + ";");
        }

        public void EmitSetAllowAnyStatusCode()
        {
            this.writer.WriteLine(this.requestInfoLocalName + ".AllowAnyStatusCode = true;");
        }

        public void EmitSetBasePath(string basePath)
        {
            this.writer.WriteLine(this.requestInfoLocalName + ".BasePath = " + QuoteString(basePath) + ";");
        }

        public void EmitAddMethodHeader(AttributeModel<HeaderAttribute> header)
        {
            this.writer.WriteLine(this.requestInfoLocalName + ".AddMethodHeader(" + QuoteString(header.Attribute.Name) + ", " +
                QuoteString(header.Attribute.Value) + ");");
        }

        public void EmitAddHeaderProperty(EmittedProperty property)
        {
            Assert(property.PropertyModel.HeaderAttribute != null);
            var attribute = property.PropertyModel.HeaderAttribute.Attribute;
            this.writer.WriteLine(this.requestInfoLocalName + ".AddPropertyHeader(" + QuoteString(attribute.Name) + ", " +
                ReferenceTo(property.PropertyModel) + ", " + QuoteString(attribute.Value) + ", " +
                QuoteString(attribute.Format) + ");");
        }

        public void EmitAddPathProperty(EmittedProperty property, PathSerializationMethod serializationMethod)
        {
            Assert(property.PropertyModel.PathAttribute != null);
            var attribute = property.PropertyModel.PathAttribute.Attribute;
            this.writer.WriteLine(this.requestInfoLocalName + ".AddPathProperty(" + EnumValue(serializationMethod) + ", " +
                QuoteString(property.PropertyModel.PathAttributeName) + ", " + ReferenceTo(property.PropertyModel) + ", " +
                QuoteString(attribute.Format) + ", " + (attribute.UrlEncode ? "true" : "false") + ");");
        }

        public void EmitAddQueryProperty(EmittedProperty property, QuerySerializationMethod serializationMethod)
        {
            Assert(property.PropertyModel.QueryAttribute != null);
            var attribute = property.PropertyModel.QueryAttribute.Attribute;
            this.writer.WriteLine(this.requestInfoLocalName + ".AddQueryProperty(" + EnumValue(serializationMethod) + ", " +
                QuoteString(property.PropertyModel.QueryAttributeName) + ", " + ReferenceTo(property.PropertyModel) + ", " +
                QuoteString(attribute.Format) + ");");
        }

        public void EmitAddHttpRequestMessagePropertyProperty(EmittedProperty property)
        {
            this.writer.WriteLine(this.requestInfoLocalName + ".AddHttpRequestMessagePropertyProperty(" +
                QuoteString(property.PropertyModel.HttpRequestMessagePropertyAttributeKey) + ", " +
                ReferenceTo(property.PropertyModel) + ");");
        }

        public void EmitSetBodyParameter(ParameterModel parameter, BodySerializationMethod serializationMethod)
        {
            this.writer.WriteLine(this.requestInfoLocalName + ".SetBodyParameterInfo(" + EnumValue(serializationMethod) + ", " +
                ReferenceTo(parameter) + ");");
        }

        public bool TryEmitAddQueryMapParameter(ParameterModel parameter, QuerySerializationMethod serializationMethod)
        {
            throw new NotImplementedException();
        }

        public void EmitAddHeaderParameter(ParameterModel parameter)
        {
            Assert(parameter.HeaderAttribute != null);
            var header = parameter.HeaderAttribute.Attribute;
            this.writer.WriteLine(this.requestInfoLocalName + ".AddHeaderParameter(" + QuoteString(header.Name) + ", " +
                ReferenceTo(parameter) + ", " + QuoteString(header.Format) + ");");
        }

        public void EmitAddPathParameter(ParameterModel parameter, PathSerializationMethod serializationMethod)
        {
            Assert(parameter.PathAttribute != null);
            var attribute = parameter.PathAttribute.Attribute;
            this.writer.WriteLine(this.requestInfoLocalName + ".AddPathParameter(" + EnumValue(serializationMethod) + ", " +
                QuoteString(parameter.PathAttributeName) + ", " + ReferenceTo(parameter) + ", " +
                QuoteString(attribute.Format) + ", " + (attribute.UrlEncode ? "true" : "false") + ");");
        }

        public void EmitAddQueryParameter(ParameterModel parameter, QuerySerializationMethod serializationMethod)
        {
            // The attribute might be null, if it's a plain parameter
            string name = parameter.QueryAttribute == null ? parameter.Name : parameter.QueryAttributeName!;
            this.writer.WriteLine(this.requestInfoLocalName + ".AddQueryParameter(" + EnumValue(serializationMethod) + ", " +
                QuoteString(name) + ", " + ReferenceTo(parameter) + ", " + QuoteString(parameter.QueryAttribute?.Attribute.Format) + ");");
        }

        public void EmitAddHttpRequestMessagePropertyParameter(ParameterModel parameter)
        {
            this.writer.WriteLine(this.requestInfoLocalName + ".AddHttpRequestMessagePropertyParameter(" +
                QuoteString(parameter.HttpRequestMessagePropertyAttributeKey) + ", " + ReferenceTo(parameter) + ");");
        }

        public void EmitAddRawQueryStringParameter(ParameterModel parameter)
        {
            this.writer.WriteLine(this.requestInfoLocalName + ".AddRawQueryParameter(" + ReferenceTo(parameter) + ");");
        }

        public bool TryEmitRequestMethodInvocation()
        {
            // Call the appropriate RequestVoidAsync/RequestAsync method, depending on whether or not we have a return type
            string? methodName = null;
            var returnType = this.methodModel.MethodSymbol.ReturnType;
            if (SymbolEqualityComparer.Default.Equals(returnType, this.wellKnownSymbols.Task))
            {
                methodName = "RequestVoidAsync";
            }

            if (methodName != null)
            {
                this.writer.WriteLine("return this." + this.requesterFieldName + "." + methodName + "(" + this.requestInfoLocalName + ");");
            }

            // This is also the end of the method
            this.writer.Indent--;
            this.writer.WriteLine("}");

            return methodName != null;
        }

        [Conditional("DEBUG")]
        private static void Assert([DoesNotReturnIf(false)] bool condition)
        {
            Debug.Assert(condition);
        }

        private static string ReferenceTo(ParameterModel parameterModel) => parameterModel.ParameterSymbol.ToDisplayString(SymbolDisplayFormats.ParameterReference);
        private static string ReferenceTo(PropertyModel propertyModel) => "this." + propertyModel.PropertySymbol.ToDisplayString(SymbolDisplayFormats.PropertyReference);
        private static string EnumValue<T>(T value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value)
                ? "global::" + typeof(T).FullName + "." + value
                : "(global::" + typeof(T).FullName + ")" + Convert.ToInt32(value);
        }
    }
}