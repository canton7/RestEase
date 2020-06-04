using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using RestEase.Implementation.Analysis;
using RestEase.Platform;
using static RestEase.Implementation.EmitEmitUtils;

namespace RestEase.Implementation.Emission
{
    internal class MethodEmitter
    {
        private readonly MethodModel methodModel;
        private readonly FieldBuilder requesterField;
        private readonly FieldBuilder? classHeadersField;
        private readonly FieldBuilder methodInfoField;
        private readonly MethodBuilder methodBuilder;
        private readonly ILGenerator ilGenerator;
        private readonly LocalBuilder requestInfoLocal;

        public MethodEmitter(TypeBuilder typeBuilder, MethodModel methodModel, int index, FieldBuilder requesterField, FieldBuilder? classHeadersField)
        {
            this.methodModel = methodModel;
            this.requesterField = requesterField;
            this.classHeadersField = classHeadersField;

            if (methodModel.IsExplicit)
            {
                this.methodBuilder = typeBuilder.DefineMethod(
                    FriendlyNameForType(methodModel.MethodInfo.DeclaringType) + "." + methodModel.MethodInfo.Name,
                    MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    methodModel.MethodInfo.ReturnType,
                    methodModel.Parameters.Select(x => x.ParameterInfo.ParameterType).ToArray());
            }
            else
            {
                this.methodBuilder = typeBuilder.DefineMethod(
                    methodModel.MethodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    methodModel.MethodInfo.ReturnType,
                    methodModel.Parameters.Select(x => x.ParameterInfo.ParameterType).ToArray());
            }

            this.AddGenericTypeParameters();
            this.AddParameters();

            if (methodModel.IsExplicit)
            {
                typeBuilder.DefineMethodOverride(this.methodBuilder, methodModel.MethodInfo);
            }
            this.ilGenerator = this.methodBuilder.GetILGenerator();

            this.methodInfoField = typeBuilder.DefineField(
                "methodInfo<>_" + index,
                typeof(MethodInfo),
                FieldAttributes.Private | FieldAttributes.Static);

            this.requestInfoLocal = this.ilGenerator.DeclareLocal(typeof(RequestInfo));

            this.AddMethodInfoInstantiation();
        }

        private void AddGenericTypeParameters()
        {
            var methodInfo = this.methodModel.MethodInfo;
            if (methodInfo.IsGenericMethodDefinition)
            {
                var genericArguments = methodInfo.GetGenericArguments();
                var builders = this.methodBuilder.DefineGenericParameters(genericArguments.Select(x => x.Name).ToArray());
                AddGenericTypeConstraints(genericArguments, builders);
            }
        }

        private void AddParameters()
        {
            var parameters = this.methodModel.MethodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameterBuilder = this.methodBuilder.DefineParameter(i + 1, parameter.Attributes, parameter.Name);
                if (parameter.HasDefaultValue)
                {
                    // So. https://github.com/dotnet/corefx/issues/26184
                    // Most of the time, we're not allowed to set `null` as the value of a struct parameter, even
                    // though the compiler happily did that, and `parameter.DefaultValue` is null. This has been
                    // fixed in CoreFx though. So I don't want to disable this behaviour all of the time, when it
                    // will work in newer CoreFx's, so instead we try, and catch the resulting exception.
                    try
                    {
                        parameterBuilder.SetConstant(parameter.DefaultValue);
                    }
                    catch (ArgumentException) { }
                }
            }
        }

        private void AddMethodInfoInstantiation()
        {
            var branchTarget = this.ilGenerator.DefineLabel();
            this.ilGenerator.Emit(OpCodes.Ldsfld, this.methodInfoField);
            this.ilGenerator.Emit(OpCodes.Brtrue, branchTarget);
            this.ilGenerator.Emit(OpCodes.Ldtoken, this.methodModel.MethodInfo);
            if (this.methodModel.MethodInfo.DeclaringType.GetTypeInfo().IsGenericType)
            {
                this.ilGenerator.Emit(OpCodes.Ldtoken, this.methodModel.MethodInfo.DeclaringType);
                this.ilGenerator.Emit(OpCodes.Call, MethodInfos.MethodBase_GetMethodFromHandle_RuntimeMethodHandle_RuntimeTypeHandle);
            }
            else
            {
                this.ilGenerator.Emit(OpCodes.Call, MethodInfos.MethodBase_GetMethodFromHandle_RuntimeMethodHandle);
            }
            this.ilGenerator.Emit(OpCodes.Castclass, typeof(MethodInfo));
            this.ilGenerator.Emit(OpCodes.Stsfld, this.methodInfoField);

            this.ilGenerator.MarkLabel(branchTarget);
        }

        public void EmitRequestInfoCreation(RequestAttributeBase requestAttribute)
        {
            this.ilGenerator.Emit(OpCodes.Ldarg_0);
            this.ilGenerator.Emit(OpCodes.Ldfld, this.requesterField);

            // For the standard HTTP methods, we can get a static instance. For others, we'll need to construct the HttpMethod
            // ourselves
            if (MethodInfos.HttpMethodProperties.TryGetValue(requestAttribute.Method, out PropertyInfo cachedPropertyInfo))
            {
                this.ilGenerator.Emit(OpCodes.Call, cachedPropertyInfo.GetMethod);
            }
            else
            {
                this.ilGenerator.Emit(OpCodes.Ldstr, requestAttribute.Method.Method);
                this.ilGenerator.Emit(OpCodes.Newobj, MethodInfos.HttpMethod_Ctor_String);
            }
            this.ilGenerator.Emit(OpCodes.Ldstr, requestAttribute.Path ?? string.Empty);
            this.ilGenerator.Emit(OpCodes.Ldsfld, this.methodInfoField);
            this.ilGenerator.Emit(OpCodes.Newobj, MethodInfos.RequestInfo_Ctor);
            this.ilGenerator.Emit(OpCodes.Stloc, this.requestInfoLocal);

            if (this.classHeadersField != null)
            {
                this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
                this.ilGenerator.Emit(OpCodes.Ldsfld, this.classHeadersField);
                this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.RequestInfo_ClassHeaders_Set);
            }
        }

        public void EmitSetCancellationToken(ParameterModel parameter)
        {
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldarg, (short)parameter.ParameterInfo.Position + 1);
            this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.RequestInfo_CancellationToken_Set);
        }

        public void EmitSetAllowAnyStatusCode()
        {
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldc_I4_1);
            this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.RequestInfo_AllowAnyStatusCode_Set);
        }

        public void EmitSetBasePath(string basePath)
        {
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldstr, basePath);
            this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.RequestInfo_BasePath_Set);
        }

        public void EmitAddMethodHeader(AttributeModel<HeaderAttribute> header)
        {
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldstr, header.Attribute.Name);
            this.LoadString(header.Attribute.Value);
            this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.RequestInfo_AddMethodHeader);
        }

        public void EmitAddHeaderProperty(EmittedProperty property)
        {
            Assert(property.PropertyModel.HeaderAttribute != null);
            var attribute = property.PropertyModel.HeaderAttribute.Attribute;
            var typedMethod = MethodInfos.RequestInfo_AddPropertyHheader.MakeGenericMethod(property.FieldBuilder.FieldType);
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldstr, attribute.Name);
            this.ilGenerator.Emit(OpCodes.Ldarg_0);
            this.ilGenerator.Emit(OpCodes.Ldfld, property.FieldBuilder);
            this.LoadString(attribute.Value);
            this.LoadString(attribute.Format);
            this.ilGenerator.Emit(OpCodes.Callvirt, typedMethod);
        }

        public void EmitAddPathProperty(EmittedProperty property, PathSerializationMethod serializationMethod)
        {
            Assert(property.PropertyModel.PathAttribute != null);
            var attribute = property.PropertyModel.PathAttribute.Attribute;
            var typedMethod = MethodInfos.RequestInfo_AddPathProperty.MakeGenericMethod(property.FieldBuilder.FieldType);
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldc_I4, (int)serializationMethod);
            this.ilGenerator.Emit(OpCodes.Ldstr, property.PropertyModel.PathAttributeName);
            this.ilGenerator.Emit(OpCodes.Ldarg_0);
            this.ilGenerator.Emit(OpCodes.Ldfld, property.FieldBuilder);
            this.LoadString(attribute.Format);
            this.ilGenerator.Emit(attribute.UrlEncode ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            this.ilGenerator.Emit(OpCodes.Callvirt, typedMethod);
        }

        public void EmitAddQueryProperty(EmittedProperty property, QuerySerializationMethod serializationMethod)
        {
            Assert(property.PropertyModel.QueryAttribute != null);
            var attribute = property.PropertyModel.QueryAttribute.Attribute;
            var typedMethod = MethodInfos.RequestInfo_AddQueryProperty.MakeGenericMethod(property.FieldBuilder.FieldType);
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldc_I4, (int)serializationMethod);
            this.ilGenerator.Emit(OpCodes.Ldstr, property.PropertyModel.QueryAttributeName);
            this.ilGenerator.Emit(OpCodes.Ldarg_0);
            this.ilGenerator.Emit(OpCodes.Ldfld, property.FieldBuilder);
            this.LoadString(attribute.Format);
            this.ilGenerator.Emit(OpCodes.Callvirt, typedMethod);
        }

        public void EmitAddHttpRequestMessagePropertyProperty(EmittedProperty property)
        {
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldstr, property.PropertyModel.HttpRequestMessagePropertyAttributeKey);
            this.ilGenerator.Emit(OpCodes.Ldarg_0);
            this.ilGenerator.Emit(OpCodes.Ldfld, property.FieldBuilder);
            if (property.FieldBuilder.FieldType.GetTypeInfo().IsValueType)
            {
                this.ilGenerator.Emit(OpCodes.Box, property.FieldBuilder.FieldType);
            }
            this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.RequestInfo_AddRequestPropertyProperty);
        }

        public void EmitSetBodyParameter(ParameterModel parameter, BodySerializationMethod serializationMethod)
        {
            var typedMethod = MethodInfos.RequestInfo_SetBodyParameterInfo.MakeGenericMethod(parameter.ParameterInfo.ParameterType);
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldc_I4, (int)serializationMethod);
            this.ilGenerator.Emit(OpCodes.Ldarg, (short)parameter.ParameterInfo.Position + 1);
            this.ilGenerator.Emit(OpCodes.Callvirt, typedMethod);
        }

        public bool TryEmitAddQueryMapParameter(ParameterModel parameter, QuerySerializationMethod serializationMethod)
        {
            var method = MakeQueryMapMethodInfo(parameter.ParameterInfo.ParameterType);
            if (method == null)
            {
                return false;
            }

            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldc_I4, (int)serializationMethod);
            this.ilGenerator.Emit(OpCodes.Ldarg, parameter.ParameterInfo.Position + 1);
            if (parameter.ParameterInfo.ParameterType.GetTypeInfo().IsValueType)
            {
                this.ilGenerator.Emit(OpCodes.Box);
            }
            this.ilGenerator.Emit(OpCodes.Callvirt, method);
            return true;
        }

        public void EmitAddHeaderParameter(ParameterModel parameter)
        {
            Assert(parameter.HeaderAttribute != null);
            var header = parameter.HeaderAttribute.Attribute;
            var typedMethod = MethodInfos.RequestInfo_AddHeaderParameter.MakeGenericMethod(parameter.ParameterInfo.ParameterType);
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldstr, header.Name);
            this.ilGenerator.Emit(OpCodes.Ldarg, parameter.ParameterInfo.Position + 1);
            this.LoadString(header.Format);
            this.ilGenerator.Emit(OpCodes.Callvirt, typedMethod);
        }

        public void EmitAddPathParameter(ParameterModel parameter, PathSerializationMethod serializationMethod)
        {
            Assert(parameter.PathAttribute != null);
            var pathParameter = parameter.PathAttribute.Attribute;
            var methodInfo = MethodInfos.RequestInfo_AddPathParameter.MakeGenericMethod(parameter.ParameterInfo.ParameterType);
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldc_I4, (int)serializationMethod);
            this.ilGenerator.Emit(OpCodes.Ldstr, parameter.PathAttributeName);
            this.ilGenerator.Emit(OpCodes.Ldarg, (short)parameter.ParameterInfo.Position + 1);
            this.LoadString(pathParameter.Format);
            this.ilGenerator.Emit(pathParameter.UrlEncode ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            this.ilGenerator.Emit(OpCodes.Callvirt, methodInfo);
        }

        public void EmitAddQueryParameter(ParameterModel parameter, QuerySerializationMethod serializationMethod)
        {
            // The attribute might be null, if it's a plain parameter
            string? name = parameter.QueryAttribute == null ? parameter.Name : parameter.QueryAttributeName;
            var methodInfo = MakeQueryParameterMethodInfo(parameter.ParameterInfo.ParameterType);
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldc_I4, (int)serializationMethod);
            this.LoadString(name);
            this.ilGenerator.Emit(OpCodes.Ldarg, parameter.ParameterInfo.Position + 1);
            this.LoadString(parameter.QueryAttribute?.Attribute.Format);
            this.ilGenerator.Emit(OpCodes.Callvirt, methodInfo);
        }

        public void EmitAddHttpRequestMessagePropertyParameter(ParameterModel parameter)
        {
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldstr, parameter.HttpRequestMessagePropertyAttributeKey);
            this.ilGenerator.Emit(OpCodes.Ldarg, parameter.ParameterInfo.Position + 1);
            if (parameter.ParameterInfo.ParameterType.GetTypeInfo().IsValueType)
            {
                this.ilGenerator.Emit(OpCodes.Box, parameter.ParameterInfo.ParameterType);
            }
            this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.RequestInfo_AddHttpRequestPropertyParameter);
        }

        public void EmitAddRawQueryStringParameter(ParameterModel parameter)
        {
            var methodInfo = MethodInfos.RequestInfo_AddRawQueryParameter.MakeGenericMethod(parameter.ParameterInfo.ParameterType);
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            this.ilGenerator.Emit(OpCodes.Ldarg, parameter.ParameterInfo.Position + 1);
            this.ilGenerator.Emit(OpCodes.Callvirt, methodInfo);
        }

        public bool TryEmitRequestMethodInvocation()
        {
            // Call the appropriate RequestVoidAsync/RequestAsync method, depending on whether or not we have a return type
            var returnType = this.methodModel.MethodInfo.ReturnType;
            var returnTypeInfo = returnType.GetTypeInfo();
            this.ilGenerator.Emit(OpCodes.Ldloc, this.requestInfoLocal);
            if (returnType == typeof(Task))
            {
                this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.Requester_RequestVoidAsync);
            }
            else if (returnTypeInfo.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var typeOfT = returnTypeInfo.GetGenericArguments()[0];
                // Now, is it a Task<HttpResponseMessage>, a Task<string>, a Task<Response<T>> or a Task<T>?
                if (typeOfT == typeof(HttpResponseMessage))
                {
                    this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.Requester_RequestWithResponseMessageAsync);
                }
                else if (typeOfT == typeof(string))
                {
                    this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.Requester_RequestRawAsync);
                }
                else if (typeOfT == typeof(Stream))
                {
                    this.ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.Requester_RequestStreamAsync);
                }
                else if (typeOfT.GetTypeInfo().IsGenericType && typeOfT.GetGenericTypeDefinition() == typeof(Response<>))
                {
                    var typedRequestWithResponseAsyncMethod = MethodInfos.Requester_RequestWithResponseAsync.MakeGenericMethod(typeOfT.GetTypeInfo().GetGenericArguments()[0]);
                    this.ilGenerator.Emit(OpCodes.Callvirt, typedRequestWithResponseAsyncMethod);
                }
                else
                {
                    var typedRequestAsyncMethod = MethodInfos.Requester_RequestAsync.MakeGenericMethod(typeOfT);
                    this.ilGenerator.Emit(OpCodes.Callvirt, typedRequestAsyncMethod);
                }
            }
            else
            {
                return false;
            }

            this.ilGenerator.Emit(OpCodes.Ret);
            return true;
        }

        private static MethodInfo MakeQueryParameterMethodInfo(Type parameterType)
        {
            Type? typeOfT = null;
            // Don't want to count string as an IEnumrable<char>...
            if (parameterType != typeof(string))
            {
                typeOfT = CollectionTypeOfType(parameterType);
            }

            // Does not implement IEnumerable<T>
            if (typeOfT == null)
            {
                return MethodInfos.RequestInfo_AddQueryParameter.MakeGenericMethod(parameterType);
            }
            else
            {
                return MethodInfos.RequestInfo_AddQueryCollectionParameter.MakeGenericMethod(typeOfT);
            }
        }

        private static MethodInfo? MakeQueryMapMethodInfo(Type queryMapType)
        {
            var nullableDictionaryTypes = DictionaryTypesOfType(queryMapType);
            if (nullableDictionaryTypes == null)
                return null;

            var dictionaryTypes = nullableDictionaryTypes.Value;

            Type? typeOfT = null;
            // Don't want to count string as an IEnumrable<char>...
            if (dictionaryTypes.Value != typeof(string))
            {
                typeOfT = CollectionTypeOfType(dictionaryTypes.Value);
            }

            if (typeOfT == null)
            {
                return MethodInfos.RequestInfo_AddQueryMap.MakeGenericMethod(dictionaryTypes.Key, dictionaryTypes.Value);
            }
            else
            {
                return MethodInfos.RequestInfo_AddQueryCollectionMap.MakeGenericMethod(dictionaryTypes.Key, dictionaryTypes.Value, typeOfT);
            }
        }

        private static KeyValuePair<Type, Type>? DictionaryTypesOfType(Type input)
        {
            foreach (var baseType in EnumerableExtensions.Concat(input, input.GetTypeInfo().GetInterfaces()))
            {
                if (baseType.GetTypeInfo().IsGenericType && baseType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    var genericArguments = baseType.GetTypeInfo().GetGenericArguments();
                    return new KeyValuePair<Type, Type>(genericArguments[0], genericArguments[1]);
                }
            }

            return null;
        }

        private static Type? CollectionTypeOfType(Type input)
        {
            foreach (var baseType in EnumerableExtensions.Concat(input, input.GetTypeInfo().GetInterfaces()))
            {
                if (baseType.GetTypeInfo().IsGenericType && baseType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return baseType.GetTypeInfo().GetGenericArguments()[0];
            }

            return null;
        }

        private void LoadString(string? str)
        {
            if (str == null)
            {
                this.ilGenerator.Emit(OpCodes.Ldnull);
            }
            else
            {
                this.ilGenerator.Emit(OpCodes.Ldstr, str);
            }
        }

        [Conditional("DEBUG")]
        private static void Assert([DoesNotReturnIf(false)] bool condition)
        {
            Debug.Assert(condition);
        }
    }
}