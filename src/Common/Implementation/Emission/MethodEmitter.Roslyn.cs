using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RestEase.Implementation.Analysis;
using RestEase.SourceGenerator.Implementation;
using static RestEase.SourceGenerator.Implementation.RoslynEmitUtils;

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
        private readonly string? methodInfoFieldName;
        private readonly string requestInfoLocalName;

        public MethodEmitter(
            MethodModel methodModel,
            IndentedTextWriter writer,
            WellKnownSymbols wellKnownSymbols,
            string qualifiedTypeName,
            string requesterFieldName,
            string? classHeadersFieldName,
            string methodInfoFieldName)
        {
            this.methodModel = methodModel;
            this.writer = writer;
            this.wellKnownSymbols = wellKnownSymbols;
            this.qualifiedTypeName = qualifiedTypeName;
            this.requesterFieldName = requesterFieldName;
            this.classHeadersFieldName = classHeadersFieldName;
            this.methodInfoFieldName = this.wellKnownSymbols.HasExpression ? methodInfoFieldName : null;
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
            if (this.methodInfoFieldName != null)
            {
                this.writer.WriteLine("private static global::System.Reflection.MethodInfo " + this.methodInfoFieldName + ";");
            }
        }

        private void EmitMethodDeclaration()
        {
            if (this.methodModel.IsExplicit)
            {
                this.writer.WriteLine(this.methodModel.MethodSymbol.ToDisplayString(SymbolDisplayFormats.ExplicitMethodDeclaration));
            }
            else
            {
                this.writer.Write("public ");
                this.writer.Write(this.methodModel.MethodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormats.MethodOrPropertyReturnType));
                this.writer.Write(" ");
                // This includes the containing type of the method, e.g. 'global::Some.Foo(...)', which we have to remove
                string methodSignature = this.methodModel.MethodSymbol.ToDisplayString(SymbolDisplayFormats.ImplicitMethodDeclaration);
                string containingType = this.methodModel.MethodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormats.ImplementedInterface);
                if (methodSignature.StartsWith(containingType))
                {
                    methodSignature = methodSignature.Substring(containingType.Length + 1); // For the '.'
                }
                else
                {
                    Debug.Assert(false);
                }
                this.writer.WriteLine(methodSignature);
            }
            this.writer.WriteLine("{");
            this.writer.Indent++;

            // I originally tried using MethodBase.GetCurrentMethod and Type.GetInterfaceMap, but that hit problems when
            // the interface type was generic. They should be automatically referencing System.Linq.Expressions.dll on
            // netstandard / netcoreapp, so just go with that for everything.
            if (this.methodInfoFieldName != null)
            {
                this.writer.WriteLine("if (" + this.qualifiedTypeName + "." + this.methodInfoFieldName + " == null)");
                this.writer.WriteLine("{");
                this.writer.Indent++;

                this.writer.Write(this.qualifiedTypeName + "." + this.methodInfoFieldName +
                    " = global::RestEase.Implementation.ImplementationHelpers.GetInterfaceMethodInfo" +
                    "<" + this.methodModel.MethodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormats.TypeParameter) + ", " +
                    this.methodModel.MethodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormats.TypeParameter) + ">(x => x." +
                    this.methodModel.MethodSymbol.ToDisplayString(SymbolDisplayFormats.SymbolName));
                if (this.methodModel.MethodSymbol.TypeArguments.Length > 0)
                {
                    this.writer.Write("<" + string.Join(", ", this.methodModel.MethodSymbol.TypeArguments
                        .Select(x => x.ToDisplayString(SymbolDisplayFormats.TypeParameter))) + ">");
                }
                this.writer.WriteLine("(" + string.Join(", ", this.methodModel.MethodSymbol.Parameters
                        .Select(x => "default(" + x.Type.ToDisplayString(SymbolDisplayFormats.TypeofParameter) + ")")) + "));");

                this.writer.Indent--;
                this.writer.WriteLine("}");
            }
        }

        public void EmitRequestInfoCreation(RequestAttributeBase requestAttribute)
        {
            this.writer.Write("var " + this.requestInfoLocalName + " = new global::RestEase.Implementation.RequestInfo(");
            if (WellKnownNames.HttpMethodProperties.TryGetValue(requestAttribute.Method, out string httpMethod))
            {
                this.writer.Write(httpMethod);
            }
            else
            {
                this.writer.Write("new global::System.Net.Http.HttpMethod(" + requestAttribute.Method.Method + ")");
            }
            this.writer.Write(", " + QuoteString(requestAttribute.Path ?? string.Empty));
            if (this.methodInfoFieldName != null)
            {
                this.writer.Write(", " + this.qualifiedTypeName + "." + this.methodInfoFieldName);
            }
            else
            {
                this.writer.Write(", null");
            }
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

        public void EmitSetBaseAddress(string baseAddress)
        {
            this.writer.WriteLine(this.requestInfoLocalName + ".BaseAddress = " + QuoteString(baseAddress) + ";");
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
            string? methodName = this.GetQueryMapMethodName(parameter.ParameterSymbol.Type);
            if (methodName == null)
                return false;

            this.writer.WriteLine(this.requestInfoLocalName + "." + methodName + "(" +
                EnumValue(serializationMethod) + ", " + ReferenceTo(parameter) + ");");
            return true;
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
            string name = parameter.QueryAttribute == null ? parameter.Name : parameter.QueryAttributeName;
            var collectionType = this.CollectionTypeOfType(parameter.ParameterSymbol.Type);
            string methodName = (collectionType == null) ? "AddQueryParameter" : "AddQueryCollectionParameter";

            this.writer.WriteLine(this.requestInfoLocalName + "." + methodName + "(" + EnumValue(serializationMethod) + ", " +
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
            else if (returnType is INamedTypeSymbol namedReturnType && namedReturnType.IsGenericType &&
                SymbolEqualityComparer.Default.Equals(namedReturnType.ConstructedFrom, this.wellKnownSymbols.TaskT))
            {
                var typeOfT = namedReturnType.TypeArguments[0];
                if (typeOfT.SpecialType == SpecialType.System_String)
                {
                    methodName = "RequestRawAsync";
                }
                else if (SymbolEqualityComparer.Default.Equals(typeOfT, this.wellKnownSymbols.HttpResponseMessage))
                {
                    methodName = "RequestWithResponseMessageAsync";
                }
                else if (SymbolEqualityComparer.Default.Equals(typeOfT, this.wellKnownSymbols.Stream))
                {
                    methodName = "RequestStreamAsync";
                }
                else if (typeOfT is INamedTypeSymbol namedTypeOfT && namedTypeOfT.IsGenericType &&
                    SymbolEqualityComparer.Default.Equals(namedTypeOfT.ConstructedFrom, this.wellKnownSymbols.ResponseT))
                {
                    methodName = "RequestWithResponseAsync<" + namedTypeOfT.TypeArguments[0].ToDisplayString(SymbolDisplayFormats.TypeParameter) + ">";
                }
                else
                {
                    methodName = "RequestAsync<" + typeOfT.ToDisplayString(SymbolDisplayFormats.TypeParameter) + ">";
                }
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
        private static string ReferenceTo(PropertyModel propertyModel)
        {
            return propertyModel.IsExplicit
                ? "((" + propertyModel.PropertySymbol.ContainingType.ToDisplayString(SymbolDisplayFormats.ImplementedInterface) +
                    ")this)." + propertyModel.PropertySymbol.ToDisplayString(SymbolDisplayFormats.PropertyReference)
                : "this." + propertyModel.PropertySymbol.ToDisplayString(SymbolDisplayFormats.PropertyReference);
        }
        private static string EnumValue<T>(T value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value)
                ? "global::" + typeof(T).FullName + "." + value
                : "(global::" + typeof(T).FullName + ")" + Convert.ToInt32(value);
        }

        private string? GetQueryMapMethodName(ITypeSymbol queryMapType)
        {
            var nullableDictionaryTypes = this.DictionaryTypesOfType(queryMapType);
            if (nullableDictionaryTypes == null)
                return null;

            var dictionaryTypes = nullableDictionaryTypes.Value;
            var collectionType = this.CollectionTypeOfType(dictionaryTypes.Value);

            // AddQueryCollectionMap sometimes needs explicit type parameters, so we'll specify them in all cases
            if (collectionType == null)
            {
                return "AddQueryMap<" + dictionaryTypes.Key.ToDisplayString(SymbolDisplayFormats.TypeParameter) +
                    ", " + dictionaryTypes.Value.ToDisplayString(SymbolDisplayFormats.TypeParameter) + ">";
            }
            else
            {
                return "AddQueryCollectionMap<" + dictionaryTypes.Key.ToDisplayString(SymbolDisplayFormats.TypeParameter) +
                    ", " + dictionaryTypes.Value.ToDisplayString(SymbolDisplayFormats.TypeParameter) +
                    ", " + collectionType.ToDisplayString(SymbolDisplayFormats.TypeParameter) + ">";
            }
        }

        private KeyValuePair<ITypeSymbol, ITypeSymbol>? DictionaryTypesOfType(ITypeSymbol input)
        {
            KeyValuePair<ITypeSymbol, ITypeSymbol>? result = null;
            if (input is INamedTypeSymbol value)
            {
                foreach (var baseType in value.AllInterfaces.Prepend(value))
                {
                    if (baseType.IsGenericType && SymbolEqualityComparer.Default.Equals(baseType.ConstructedFrom, this.wellKnownSymbols.IDictionaryKV))
                    {
                        result = new KeyValuePair<ITypeSymbol, ITypeSymbol>(
                            baseType.TypeArguments[0], baseType.TypeArguments[1]);
                        break;
                    }
                }
            }
            else if (input is ITypeParameterSymbol typeParameter)
            {
                // Are any of its constraints suitable dictionaries
                foreach (var constraint in typeParameter.ConstraintTypes)
                {
                    result = this.DictionaryTypesOfType(constraint);
                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        private ITypeSymbol? CollectionTypeOfType(ITypeSymbol input)
        {
            ITypeSymbol? collectionType = null;
            // Don't want to count string as an IEnumerable<char>...
            if (input.SpecialType != SpecialType.System_String &&
                input is INamedTypeSymbol value)
            {
                foreach (var baseType in input.AllInterfaces.Prepend(input))
                {
                    if (baseType is INamedTypeSymbol namedBaseType &&
                        namedBaseType.IsGenericType &&
                        SymbolEqualityComparer.Default.Equals(namedBaseType.ConstructedFrom, this.wellKnownSymbols.IEnumerableT))
                    {
                        collectionType = namedBaseType.TypeArguments[0];
                        break;
                    }
                }
            }
            else if (input is IArrayTypeSymbol arraySymbol)
            {
                collectionType = arraySymbol.ElementType;
            }
            else if (input is ITypeParameterSymbol typeParameter)
            {
                // Are any of its constraints IEnumerable<T>?
                foreach (var constraint in typeParameter.ConstraintTypes)
                {
                    collectionType = this.CollectionTypeOfType(constraint);
                    if (collectionType != null)
                    {
                        break;
                    }
                }
            }

            return collectionType;
        }
    }
}