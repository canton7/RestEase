using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using RestEase.Platform;

namespace RestEase.Implementation
{
    internal static class MethodInfos
    {
        public static MethodInfo IDisposable_Dispose { get; } = typeof(IDisposable).GetTypeInfo().GetMethod("Dispose")!;
        public static ConstructorInfo HttpMethod_Ctor_String { get; } = typeof(HttpMethod).GetTypeInfo().GetConstructor(new[] { typeof(string) })!;
        public static MethodInfo MethodBase_GetMethodFromHandle_RuntimeMethodHandle { get; } = typeof(MethodBase).GetTypeInfo().GetMethods().Single(x => x.Name == "GetMethodFromHandle" && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(RuntimeMethodHandle));
        public static MethodInfo MethodBase_GetMethodFromHandle_RuntimeMethodHandle_RuntimeTypeHandle { get; } = typeof(MethodBase).GetTypeInfo().GetMethods().Single(x => x.Name == "GetMethodFromHandle" && x.GetParameters().Length == 2 && x.GetParameters()[0].ParameterType == typeof(RuntimeMethodHandle) && x.GetParameters()[1].ParameterType == typeof(RuntimeTypeHandle));
        public static ConstructorInfo KeyValuePair_Ctor_String_String { get; } = typeof(KeyValuePair<string, string>).GetTypeInfo().GetConstructor(new[] { typeof(string), typeof(string) })!;
        public static ConstructorInfo RequestInfo_Ctor { get; } = typeof(RequestInfo).GetTypeInfo().GetConstructor(new[] { typeof(HttpMethod), typeof(string), typeof(MethodInfo) })!;
        public static MethodInfo RequestInfo_ClassHeaders_Set { get; } = typeof(RequestInfo).GetTypeInfo().GetProperty("ClassHeaders")!.SetMethod!;
        public static MethodInfo RequestInfo_CancellationToken_Set { get; } = typeof(RequestInfo).GetTypeInfo().GetProperty("CancellationToken")!.SetMethod!;
        public static MethodInfo RequestInfo_AddPropertyHheader { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("AddPropertyHeader")!;
        public static MethodInfo RequestInfo_AddPathProperty { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("AddPathProperty")!;
        public static MethodInfo RequestInfo_AddQueryProperty { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("AddQueryProperty")!;
        public static MethodInfo RequestInfo_AddRequestPropertyProperty { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("AddHttpRequestMessagePropertyProperty")!;
        public static MethodInfo RequestInfo_AllowAnyStatusCode_Set { get; } = typeof(RequestInfo).GetTypeInfo().GetProperty("AllowAnyStatusCode")!.SetMethod!;
        public static MethodInfo RequestInfo_BaseAddress_Set { get; } = typeof(RequestInfo).GetTypeInfo().GetProperty("BaseAddress")!.SetMethod!;
        public static MethodInfo RequestInfo_BasePath_Set { get; } = typeof(RequestInfo).GetTypeInfo().GetProperty("BasePath")!.SetMethod!;
        public static MethodInfo RequestInfo_AddMethodHeader { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("AddMethodHeader")!;
        public static MethodInfo RequestInfo_SetBodyParameterInfo { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("SetBodyParameterInfo")!;
        public static MethodInfo RequestInfo_AddQueryMap { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("AddQueryMap")!;
        public static MethodInfo RequestInfo_AddQueryCollectionMap = typeof(RequestInfo).GetTypeInfo().GetMethod("AddQueryCollectionMap")!;
        public static MethodInfo RequestInfo_AddHeaderParameter { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("AddHeaderParameter")!;
        public static MethodInfo RequestInfo_AddPathParameter { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("AddPathParameter")!;
        // These two methods have the same signature, which is very useful...
        public static MethodInfo RequestInfo_AddQueryParameter { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("AddQueryParameter")!;
        public static MethodInfo RequestInfo_AddQueryCollectionParameter { get; } = typeof(RequestInfo).GetTypeInfo().GetMethod("AddQueryCollectionParameter")!;
        public static MethodInfo RequestInfo_AddHttpRequestPropertyParameter = typeof(RequestInfo).GetTypeInfo().GetMethod("AddHttpRequestMessagePropertyParameter")!;
        public static MethodInfo RequestInfo_AddRawQueryParameter = typeof(RequestInfo).GetTypeInfo().GetMethod("AddRawQueryParameter")!;
        public static MethodInfo Requester_RequestVoidAsync { get; } = typeof(IRequester).GetTypeInfo().GetMethod("RequestVoidAsync")!;
        public static MethodInfo Requester_RequestWithResponseMessageAsync { get; } = typeof(IRequester).GetTypeInfo().GetMethod("RequestWithResponseMessageAsync")!;
        public static MethodInfo Requester_RequestRawAsync { get; } = typeof(IRequester).GetTypeInfo().GetMethod("RequestRawAsync")!;
        public static MethodInfo Requester_RequestStreamAsync { get; } = typeof(IRequester).GetTypeInfo().GetMethod("RequestStreamAsync")!;
        public static MethodInfo Requester_RequestWithResponseAsync { get; } = typeof(IRequester).GetTypeInfo().GetMethod("RequestWithResponseAsync")!;
        public static MethodInfo Requester_RequestAsync { get; } = typeof(IRequester).GetTypeInfo().GetMethod("RequestAsync")!;

        public static IReadOnlyDictionary<HttpMethod, PropertyInfo> HttpMethodProperties { get; } = new Dictionary<HttpMethod, PropertyInfo>()
        {
            { HttpMethod.Delete, typeof(HttpMethod).GetTypeInfo().GetProperty("Delete")! },
            { HttpMethod.Get, typeof(HttpMethod).GetTypeInfo().GetProperty("Get")! },
            { HttpMethod.Head, typeof(HttpMethod).GetTypeInfo().GetProperty("Head")! },
            { HttpMethod.Options, typeof(HttpMethod).GetTypeInfo().GetProperty("Options")! },
            { HttpMethod.Post, typeof(HttpMethod).GetTypeInfo().GetProperty("Post")! },
            { HttpMethod.Put, typeof(HttpMethod).GetTypeInfo().GetProperty("Put")! },
            { HttpMethod.Trace, typeof(HttpMethod).GetTypeInfo().GetProperty("Trace")! },
            { PatchAttribute.PatchMethod, typeof(PatchAttribute).GetTypeInfo().GetProperty("PatchMethod")! },
        };
    }
}
