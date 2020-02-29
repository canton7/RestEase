![Project Icon](icon.png) RestEase
==================================

[![NuGet](https://img.shields.io/nuget/v/RestEase.svg)](https://www.nuget.org/packages/RestEase/)
[![Build status](https://ci.appveyor.com/api/projects/status/5ap27qo5d7tm2o5n?svg=true)](https://ci.appveyor.com/project/canton7/restease)

RestEase is a little type-safe REST API client library for .NET Framework 4.0 and higher, which aims to make interacting with remote REST endpoints easy, without adding unnecessary compexity.

Almost every aspect of RestEase can be overridden and customized, leading to a large level of flexibility.

To use it, you define an interface which represents the endpoint you wish to communicate with (more on that in a bit), where methods on that interface correspond to requests that can be made on it.
RestEase will then generate an implementation of that interface for you, and by calling the methods you defined, the appropriate requests will be made.

RestEase is built on top of [HttpClient](https://msdn.microsoft.com/en-us/library/system.net.http.httpclient%28v=vs.118%29.aspx) and is deliberately a "leaky abstraction": it is easy to gain access to the full capabilities of HttpClient, giving you control and flexibility, when you need it.

RestEase is heavily inspired by [Ana Betts' Refit](https://github.com/paulcbetts/refit), which in turn is inspired by Retrofit.

### Table of Contents

1. [Installation](#installation)
2. [Quick Start](#quick-start)
3. [Request Types](#request-types)
4. [Return Types](#return-types)
5. [Query Parameters](#query-parameters)
  1. [Constant Query Parameters](#constant-query-parameters)
  2. [Variable Query Parameters](#variable-query-parameters)
    1. [Serialization of Variable Query Parameters](#serialization-of-variable-query-parameters) 
  3. [Query Parameters Map](#query-parameters-map)
6. [Path Parameters](#path-parameters)
7. [Body Content](#body-content)
  1. [URL Encoded Bodies](#url-encoded-bodies)
8. [Response Status Codes](#response-status-codes)
9. [Cancelling Requests](#cancelling-requests)
10. [Headers](#headers)
  1. [Constant Interface Headers](#constant-interface-headers)
  2. [Variable Interface Headers](#variable-interface-headers)
  3. [Constant Method Headers](#constant-method-headers)
  4. [Variable Method Headers](#variable-method-headers)
  5. [Redefining Headers](#redefining-headers)
11. [Controlling Serialization and Deserialization](#controlling-serialization-and-deserialization)
  1. [Custom `JsonSerializerSettings`](#custom-jsonserializersettings)
  2. [Custom Serializers and Deserializers](#custom-serializers-and-deserializers)
    1. [Deserializing responses: `IResponseDeserializer`](#deserializing-responses-iresponsedeserializer)
    2. [Serializing request bodies: `IRequestBodySerializer`](#serializing-request-bodies-irequestbodyserializer)
    3. [Serializing request parameters: `IRequestQueryParamSerializer`](#serializing-request-parameters-irequestqueryparamserializer)
12. [Controlling the Requests](#controlling-the-requests)
  1. [`RequestModifier`](#requestmodifier)
  2. [Custom `HttpClient`](#custom-httpclient)
13. [Customizing RestEase](#customizing-restease)
14. [Interface Accessibility](#interface-accessibility)
15. [Using Generic Interfaces](#using-generic-interfaces)
16. [Interface Inheritance](#interface-inheritance)
17. [FAQs](#faqs)
18. [Comparison to Refit](#comparison-to-refit)


Installation
------------

[RestEase is available on NuGet](https://www.nuget.org/packages/RestEase).

Either open the package console and type:

```
PM> Install-Package RestEase
```

Or right-click your project -> Manage NuGet Packages... -> Online -> search for RestEase in the top right.

Symbols are available.
In Visual Studio, go to Debug -> Options and Settings -> General, and make the following changes:

1. Turn **off** "Enable Just My Code"
2. Turn **off** "Enable .NET Framework source stepping". Yes, it is misleading, but if you don't, then Visual Studio will ignore your custom server order and only use its own servers.
3. Turn **on** "Enable source server support". You may have to OK a security warning.


Quick Start
-----------

To start, first create an public interface which represents the endpoint you wish to make requests to.
Please note that it does have to be public, or you must add RestEase as a friend assembly, see [Interface Accessibility below](#interface-accessibility).

```csharp
// Define an interface representing the API
public interface IGitHubApi
{
    // All interface methods must return a Task or Task<T>. We'll discuss what sort of T in more detail below.

    // The [Get] attribute marks this method as a GET request
    // The "users" is a relative path the a base URL, which we'll provide later
    [Get("users")]
    Task<List<User>> GetUsersAsync();
}

// Create an implementation of that interface
// We'll pass in the base URL for the API
IGitHubApi api = RestClient.For<IGitHubApi>("http://api.github.com");

// Now we can simply call methods on it
// Sends a GET request to http://api.github.com/users
List<User> users = await api.GetUsersAsync();
```


Request Types
-------------

See the `[Get("path")]` attribute used above?
That's how you mark that method as being a GET request.
There are a number of other attributes you can use here - in fact, there's one for each type of request: `[Post("path")]`, `[Delete("path")]`, etc.
Use whichever one you need to.

The argument to `[Get]` (or `[Post]`, or whatever) is typically a relative path, and will be relative to the base uri that you provide to `RestClient.For<T>`.
(You *can* specify an absolute path here if you need to, in which case the base uri will be ignored).


Return Types
------------

Your interface methods may return one of the following types:

 - `Task`: This method does not return any data, but the task will complete when the request has completed
 - `Task<T>` (where `T` is not one of the types listed below): This method will deserialize the response into an object of type `T`, using Json.NET (or a custom deserializer, see [Controlling Serialization and Deserialization below](#controlling-serialization-and-deserialization)).
 - `Task<string>`: This method returns the raw response, as a string
 - `Task<HttpResponseMessage>`: This method returns the raw [`HttpResponseMessage`](https://msdn.microsoft.com/en-us/library/system.net.http.httpresponsemessage%28v=vs.118%29.aspx) resulting from the request. It does not do any deserialiation
 - `Task<Response<T>>`: This method returns a `Response<T>`. A `Response<T>` contains both the deserialied response (of type `T`), but also the `HttpResponseMessage`. Use this when you want to have both the deserialized response, and access to things like the response headers

Non-async methods are not supported (use `.Wait()` or `.Result` as appropriate if you do want to make your request synchronous).


Query Parameters
----------------

It is very common to want to include query parameters in your request (e.g. `/foo?key=value`), and RestEase makes this easy.

### Constant Query Parameters

The most basic type of query parameter is a constant - the value never changes.
For these, simply put the query parameter as part of the URL:

```csharp
public interface IGitHubApi
{
   [Get("users/list?sort=desc")]
   Task<List<User>> GetUsersAsync();
}
```

### Variable Query Parameters

Any parameters to a method which are:

 - Decorated with the `[Query]` attribute, or
 - Not decorated at all

will be interpreted as query parameters.

The name of the parameter will be used as the key, unless an argument is passed to `[Query("key")]`, in which case that will be used instead.

For example:

```csharp
public interface IGitHubApi
{
    [Get("user")]
    Task<User> FetchUserAsync(int userid);

    // Is the same as:

    [Get("user")]
    Task<User> FetchUserAsync([Query] int userid);

    // Is the same as:
    // (Note the casing of the parameter name)

    [Get("user")]
    Task<User> FetchUserAsync([Query("userid")] int userId);
}

IGithubApi api = RestClient.For<IGithubApi>("http://api.github.com");

// Requests http://api.github.com/user?userId=3
await api.FetchUserAsync(3);
```

You can have duplicate keys if you want:

```csharp
public interface ISomeApi
{
    [Get("search")]
    Task<SearchResult> SearchAsync([Query("filter")] string filter1, [Query("filter")] string filter2);
}

ISomeApi api = RestClient.For<ISomeApi>("http://api.example.com");

// Requests http://somenedpoint.com/search?filter=foo&filter=bar
await api.SearchAsync("foo", "bar");
```

You can also have an array of query parameters:

```csharp
public interface ISomeApi
{
    // You can use IEnumerable<T>, or any type which implements IEnumerable<T>

    [Get("search")]
    Task<SearchResult> SearchAsync([Query("filter")] IEnumerable<string> filters);
}

ISomeApi api = RestClient.For<ISomeApi>("http://api.example.com");

// Requests http://api.exapmle.com/search?filter=foo&filter=bar&filter=baz
await api.SearchAsync(new[] { "foo", "bar", "baz" });
```

#### Serialization of Variable Query Parameters

By default, query parameter values will be serialized by calling `ToString()` on them.
This means that the primitive types most often used as query parameters - `string`, `int`, etc - are serialized correctly.

However, some APIs require that you send e.g. JSON as a query parameter.
In this case, you can mark the parameter for custom serialization using `QuerySerializationMethod.Serialized`, and further control it by using a [custom serializer](#custom-serializers-and-deserializers).

For example:
```csharp
public class SearchParams
{
    public string Term { get; set; }
    public string Mode { get; set; }
}

public interface ISomeApi
{
    [Get("search")]
    Task<SearchResult> SearchAsync([Query(QuerySerializationMethod.Serialized)] SearchParams param);
}

ISomeApi = RestClient.For<ISomeApi>("http://api.example.com");
// Requests http://api.example.com/search?params={"Term": "foo", "Mode": "basic"}
await api.SearchAsync(new SearchParams() { Term = "foo", Mode = "basic" });
```

You can also specify the default serialization method for an entire api by specifying `[SerializationMethods(Query = QuerySerializationMethod.Serialized)]` on the interface, or for all parameters in a given method by specifying it on the method, for example:

```csharp
[SerializationMethods(Query = QuerySerializationMethods.Serialized)]
public interface ISomeApi
{
    [Get("search")]
    [SerializationMethods(Query = QuerySerializationMethod.ToString)]
    Task<SearchResult> SearchWithToStringAsync([Query] SearchParams param);

    [Get("search")]
    Task<SearchResult> SearchWithSerializedAsync([Query] SearchParams param);
}
```


### Query Parameters Map

Sometimes you have a load of query parameters, or they're generated dynamically, etc.
In this case, you may want to supply a dictionary of query parameters, rather than specifying a load of method parameters.

To facilitate this, you may decorate one or more method parameters with `[QueryMap]`.
The parameter type must be an `IDictionary<TKey, TValue>`.

Query maps are handled the same way as other query parameters: serialization, handling of enumerables, null values, etc, behave the same.
You can control whether values are serialized using a custom serializer or `ToString()` using e.g. `[QueryMap(QuerySerializationMethod.Serialized)]`.

For example:

```csharp
public interface ISomeApi
{
    [Get("search")]
    // I've used IDictionary<string, string[]> here, but you can use whatever type parameters you like,
    // or any type which implements IDictionary<TKey, TValue>
    Task<SearchResult> SearchBlogPostsAsync([QueryMap] IDictionary<string, string[]> filters);
}

var api = RestClient.For<ISomeApi>("http://api.example.com");
var filters = new Dictionary<string, string[]>()
{
    { "title", new[] { "bobby" } },
    { "tag", new[] { "c#", "programming" } }
};

// Requests http://api.example.com/search?title=bobby&tag=c%23&tag=programming
var searchResults = await api.SearchBlogPostsAsync(filters);
```


Path Parameters
---------------

Sometimes you also want to be able to control some parts of the path itself, rather than just the query parameters.
This is done using placeholders in the path, and corresponding method parameters decorated with `[Path]`.

For example:

```csharp
public interface ISomeApi
{
    [Get("user/{userId}")]
    Task<User> FetchUserAsync([Path] string userId);
}

ISomeApi api = RestClient.For<ISomeApi>("http://api.example.com");

// Requests http://api.example.com/user/fred
await api.FetchUserAsync("fred");
```

As with `[Query]`, the name of the placeholder to substitute is determined by the name of the parameter.
If you want to override this, you can pass an argument to `[Path("placeholder")]`, e.g.:

```csharp
public interface ISomeApi
{
    [Get("user/{userId}")]
    Task<User> FetchUserAsync([Path("userId")] string idOfTheUser);
}
```

Every placeholder must have a corresponding parameter, and every parameter must relate to a placeholder.


Body Content
------------

If you're sending a request with a body, you can specify that one of the parameters to your method contains the body you want to send, using the `[Body]` attribute.

```csharp
public interface ISomeApi
{
    [Post("users/new")]
    Task CreateUserAsync([Body] User user);
}
```

Exactly how this will be serialized depends on the type of parameters:

 - If the type is `Stream`, then the content will be streamed via [`StreamContent`](https://msdn.microsoft.com/en-us/library/system.net.http.streamcontent%28v=vs.118%29.aspx).
 - If the type is `String`, then the string will be used directly as the content (using [`StringContent`](https://msdn.microsoft.com/en-us/library/system.net.http.stringcontent%28v=vs.118%29.aspx)).
 - If the parameter has the attribute `[Body(BodySerializationMethod.UrlEncoded)]`, then the content will be URL-encoded ([see below](#url-encoded-bodies)).
 - If the type is a [`HttpContent`](https://msdn.microsoft.com/en-us/library/system.net.http.httpcontent%28v=vs.118%29.aspx) (or one of its subclasses), then it will be used directly. This is useful for advanced scenarios
 - Otherwise, the parameter will be serialized as JSON (by default, or you can customize this if you want, see [Controlling Serialization and Deserialization](#controlling-serialization-and-deserialization)).


### URL Encoded Bodies

For APIs which take form posts (i.e. serialized as `application/x-www-form-urlencoded`), initialize the `[Body]` attribute with `BodySerializationMethod.UrlEncoded`.
This parameter must implement `IDictionary` or `IDictionary<TKey, TValue>`.

If any of the values implement `IEnumerable`, then they will be serialized as an array of values.

For example:

```csharp
public interface IMeasurementProtocolApi
{
    [Post("collect")]
    Task CollectAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);
}

var data = new Dictionary<string, object> {
    {"v", 1}, 
    {"tids", new[] { "UA-1234-5", "UA-1234-6" }, 
    {"cid", new Guid("d1e9ea6b-2e8b-4699-93e0-0bcbd26c206c")}, 
    {"t", "event"},
};

// Serialized as: v=1&tids=UA-1234-5&tids=UA-1234-6&cid=d1e9ea6b-2e8b-4699-93e0-0bcbd26c206c&t=event
await api.CollectAsync(data);
 ```

You can also control the default body serialization method for an entire API by specifying `[SerializationMethods(BodySerializationMthod.UrlEncoded)]` on the interface itself:

```csharp
[SerializationMethods(BodySerializationMethod.UrlEncoded)]
public interface ISomeApi
{
    [Post("collect")]
    Task CollectAsync([Body] Dictionary<string, object> data);
}
```


Response Status Codes
---------------------

By default, any response status code which does not indicate success (as indicated by [`HttpResponseMessage.IsSuccessStatusCode`](https://msdn.microsoft.com/en-us/library/system.net.http.httpresponsemessage.issuccessstatuscode%28v=vs.118%29.aspx)) will cause an `ApiException` to be thrown.

This is usually what you want (you don't want to try and parse the result of a failed request), but sometimes you're expecting failure.

In this case, you can apply `[AllowAnyStatusCode]` to you method, or indeed to the whole interface, to suppress this behaviour. If you do this, then you probably want to make your method return either a `HttpResponseMessage` or a `Response<T>` (see [Return Types](#return-types)) so you can examine the response code yourself.

For example:

```csharp
public interface ISomeApi
{
    [Get("users/{userId}")]
    [AllowAnyStatusCode]
    Task<Response<User>> FetchUserThatMayNotExistAsync([Path] int userId);
}

ISomeApi api = RestClient.For<ISomeApi>("http://api.example.com");

var response = await api.FetchUserThatMayNotExistAsync(3);
if (response.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
{
    // User wasn't found
}
else
{
    var user = response.GetContent();
    // ...
}
```


Cancelling Requests
-------------------

If you want to be able to cancel a request, pass a `CancellationToken` as one of the method paramters.

```csharp
public interface ISomeApi
{
    [Get("very-large-response")]
    Task<LargeResponse> GetVeryLargeResponseAsync(CancellationToken cancellationToken);
}
```


Headers
-------

Specifying headers is actually a surprisingly large topic, and can be done in several ways, depending on the precise behaviour you want.

### Constant Interface Headers

If you want to have a header that applies to every single request, and whose value is fixed, use a constant interface headers.
These are specified as `[Header("Name", "Value")]` attributes on the interface.

For example:

```csharp
[Header("User-Agent", "RestEase")]
[Header("Cache-Control", "no-cache")]
public interface IGitHubApi
{
    [Get("users")]
    Task<List<User>> GetUsersAsync();
}
```

### Variable Interface Headers

If you want to have a header that applies to every single request, and whose value is variable, then use a variable interface header.
These are specifed using properties, using a `[Header("Name")]` attribute on that property.

For example:

```csharp
public interface ISomeApi
{
    [Header("X-API-Key")]
    string ApiKey { get; set; }

    [Get("users/{userId}")]
    Task<User> FetchUserId([Path] string userId);
}

ISomeApi api = RestClient.For<ISomeApi>("http://api.example.com")
api.ApiKey = "The-API-KEY-value";
// ...
```

For nullable property types, you can also specify a default (which will be used when the property is null):

```csharp
public interface ISomeApi
{
    [Header("X-API-Key", "None")]
    string ApiKey { get; set; }

    [Get("users/{userId}")]
    Task<User> FetchUserId([Path] string userId);
}

ISomeApi api = RestClient.For<ISomeApi>("http://api.example.com")

// "X-API-Key: None"
var user = await api.FetchUserAsync("bob");
```

### Constant Method Headers

If you want to have a header which only applies to a particular method, and whose value never changes, then use a constant method header.
Like constant interface headers, these are defined in their entirety using an attribute.
However, instead of applying the attribute to the interface, you apply it to the method.

```csharp
public interface IGitHubApi
{
    [Header("User-Agent", "RestEase")]
    [Header("Cache-Control", "no-cache")]
    [Get("users")]
    Task<List<User>> GetUsersAsync();

    // This method doesn't have any headers applied
    [Get("users/{userId}")]
    Task<User> GetUserAsync([Path] string userId);
}
```

### Variable Method Headers

Finally, you can have headers which only apply to a single method and whose values are variable.
These consist of a `[Header("Name")]` attribute applied to a method parameter.

```csharp
public interface ISomeApi
{
    [Get("users")]
    Task<List<User>> GetUsersAsync([Header("Authorization")] string authorization);
}
```

### Redefining Headers

You've probably noticed that there are 4 places you can define a header: on the interface, as a property, on a method, and as a parameter (or, Constant Interface Headers, Variable Interface Headers, Constant Method Headers, and Variable Method Headers, respectively).
There are rules specifying how headers from different places are merged.

Constant and Variable Interface headers are merged, as are Constant and Variable Method headers.
That is, if a header is supplied both as an attribute on the interface, and as a property, that header will have multiple values.

Method headers will replace Interface headers.
If you have the same header on a method and on the interface, then the header on the method will replace the one on the interface.

Another rule is that a header with a value of `null` will not be added, but can still replace a previously-defined header of the same name.

Example time:

```csharp
[Header("X-InterfaceOnly", "InterfaceValue")]
[Header("X-InterfaceAndParamater", "InterfaceValue")]
[Header("X-InterfaceAndMethod", "InterfaceValue"]
[Header("X-InterfaceAndParameter", "InterfaceValue"]
[Header("X-InterfaceAndMethod-ToBeRemoved", "InterfaceValue")]
public interface ISomeApi
{
    [Header("X-ParameterOnly")]
    string ParameterOnlyHeader { get; set; }

    [Header("X-InterfaceAndParameter")]
    string InterfaceAndParameterHeader { get; set; }

    [Header("X-ParameterAndMethod")]
    string ParameterAndMethodHeader { get; set; }

    [Get("url")]
    [Header("X-MethodOnly", "MethodValue")]
    [Header("X-MethodAndParameter", "MethodValue")]
    [Header("X-ParameterAndMethod", "MethodValue")]
    [Header("X-InterfaceAndMethod-ToBeRemoved", null)]
    Task DoSomethingAsync(
        [Header("X-ParameterOnly")] string parameterOnly,
        [Header("X-MethodAndParameter")] string methodAndParameter,
        [Header("X-InterfaceAndParameter")] string interfaceAndParameter
    );
}

ISomeApi api = RestClient.For<ISomeApi>("http://api.example.com");

api.ParameterOnlyHeader = "ParameterValue";
api.InterfaceAndParameterHeader = "ParameterValue";
api.ParameterAndMethodHeader = "ParameterValue";

await api.DoSomethingAsync("ParameterValue", "ParameterValue", "ParameterValue");

// Has the following headers:
// X-InterfaceOnly: InterfaceValue
// X-InterfaceAndParameter: InterfaceValue, ParameterValue
// X-InterfaceAndMethod: MethodValue
// X-InterfaceAndParameter: ParameterValue

// X-ParameterAndMethod: MethodValue

// X-MethodOnly: MethodValue
// X-MethodAndParameter: MethodValue, ParameterValue

// X-ParameterAndMethod-ToBeRemoved isn't set, because it was removed

```


Controlling Serialization and Deserialization
---------------------------------------------

By default, RestEase will use [Json.NET](http://www.newtonsoft.com/json) to deserialize responses, and serialize request bodies and query parameters.
However, you can change this, either by specifying custom `JsonSerializerSettings`, or by providing your own serializers / deserializers

### Custom `JsonSerializerSettings`

If you want to specify your own `JsonSerializerSettings`, you can do this by constructing a new `RestClient`, assigning `JsonSerializerSettings`, then calling `For<T>()` to obtain an implementation of your interface, for example:

```csharp
var settings = new JsonSerializerSettings()
{
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
    Converters = { new StringEnumConverter() }
};

var api = new RestClient("http://api.example.com")
{
    JsonSerializerSettings = settings
}.For<ISomeApi>();
```

### Custom Serializers and Deserializers

You can completely customize how requests are serialized, and responses deserialized, by providing your own serializer/deserializer implementations:

 - To control how responses are deserialized, implement [`IResponseDeserializer`](https://github.com/canton7/RestEase/blob/master/src/RestEase/IResponseDeserializer.cs)
 - To control how request bodies are serialized, implement [`IRequestBodySerializer`](https://github.com/canton7/RestEase/blob/master/src/RestEase/IRequestBodySerializer.cs)
 - To control how request query parameters are serialized, implement [`IRequestQueryParamSerializer`](https://github.com/canton7/RestEase/blob/master/src/RestEase/IRequestQueryParamSerializer.cs)

You can, of course, provide a custom implementation of only one of these, or all of them, or any number in between.

#### Deserializing responses: `IResponseDeserializer`

This class has a single method, which is called whenever a response is received which needs deserializing.
It is passed the `HttpResponseMessage` (so you can read headers, etc, if you want) and its `string` content which has already been asynchronously read.

For an example, see [`JsonResponseDeserializer`](https://github.com/canton7/RestEase/blob/master/src/RestEase/JsonResponseDeserializer.cs).

To tell RestEase to use it, you must create a new `RestClient`, assign its `ResponseDeserializer` property, then call `For<T>()` to get an implementation of your interface.

```csharp
// This API returns XML

public class XmlResponseDeserializer : IResponseDeserializer
{
    public T Deserialize<T>(string content, HttpResponseMessage response)
    {
        // Consider caching generated XmlSerializers
        var serializer = new XmlSerializer(typeof(T));

        using (var stringReader = new StringReader(content))
        {
            return (T)serializer.Deserialize(stringReader);
        }
    }
}

// ...

var api = new RestClient("http://api.example.com")
{
    ResponseDeserializer = new XmlResponseDeserializer()
}.For<ISomeApi>();
```

#### Serializing request bodies: `IRequestBodySerializer`

This class has a single method, which is called whenever a request body requires serialization (i.e. is decorated with `[Body(BodySerializationMethod.Serialized)]`).
It returns any `HttpContent` subclass you like, although `StringContent` is likely to be a common choice.

When writing an `IRequestBodySerializer`'s `SerializeBody` implementation, you may choose to provide some default headers, such as `Content-Type`.
These will be overidden by any `[Header]` attributes.

For an example, see [`JsonRequestBodySerializer`](https://github.com/canton7/RestEase/blob/master/src/RestEase/JsonRequestBodySerializer.cs).

To tell RestEase to use it, you must create a new `RestClient`, assign its `RequestBodySerializer` property, then call `For<T>()` to get an implementation of your interface.

For example:

```csharp
public class XmlRequestBodySerializer : IRequestBodySerializer
{
    public HttpContent SerializeBody<T>(T body)
    {
        if (body == null)
            return null;

        // Consider caching generated XmlSerializers
        var serializer = new XmlSerializer(typeof(T));

        using (var stringWriter = new StringWriter())
        {
            serializer.Serialize(stringWriter, body);
            var content = new StringContent(stringWriter.ToString());
            // Set the default Content-Type header to application/xml
            content.Headers.ContentType.MediaType = "application/xml";
            return content;
        }
    }
}

// ...

var api = new RestClient("http://api.example.com")
{
    RequestBodySerializer = new XmlRequestBodySerializer()
}.For<ISomeApi>();
```

#### Serializing request parameters: `IRequestQueryParamSerializer`

This class has two methods: one is called whenever a scalar query parameter requires serialization (i.e. is decorated with `[Query(QuerySerializationMethod.Serialized)]`); the other is called whenever a collection of query parameters (that is, the query parameter has type `IEnumerable<T>` for some `T`) requires serialization.

Both of these methods want you to return an `IEnumerable<KeyValuePair<string, string>>`, where each key corresponds to the name of a query name/value pair, and each value corresponds to the value.
For example:

```csharp
return new[]
{
    new KeyValuePair<string, string>("foo", "bar"),
    new KeyValuePair<string, string>("foo", "baz"),
    new KeyValuePair<string, string>("yay", "woo")
}

// Will get serialized to '...?foo=bar&foo=baz&yay=woo'
```

It is unlikely that you will return more than one `KeyValuePair` from the method which serializes scalar query parameters, but the flexibility is there.

For an example, see [`JsonRequestQueryParamSerializer`](https://github.com/canton7/RestEase/blob/master/src/RestEase/JsonRequestQueryParamSerializer.cs).

To tell RestEase to use it, you must create a new `RestClient`, assign its `RequestQueryParamSerializer` property, then call `For<T>()` to get an implementation of your interface.

For example:

```csharp
// It's highly unlikely that you'll get an API which requires xml-encoded query
// parameters, but for the sake of an example:

public class XmlRequestQueryParamSerializer : IRequestQueryParamSerializer
{
    public IEnumerable<KeyValuePair<string, string>> SerializeQueryParam<T>(string name, T value)
    {
        if (value == null)
            yield break;

        // Consider caching generated XmlSerializers
        var serializer = new XmlSerializer(typeof(T));

        using (var stringWriter = new StringWriter())
        {
            serializer.Serialize(stringWriter, value);
            yield return new KeyValuePair<string, string>(name, stringWriter.ToString()));
        }
    }

    public IEnumerable<KeyValuePair<string, string>> SerializeQueryCollectionParam<T>(string name, IEnumerable<T> values)
    {
        if (values == null)
            yield break;

        // Consider caching generated XmlSerializers
        var serializer = new XmlSerializer(typeof(T));

        foreach (var value in values)
        {
            if (value != null)
            {
                using (var stringWriter = new StringWriter())
                {
                    serializer.Serialize(stringWriter, value);
                    yield return new KeyValuePair<string, string>(name, stringWriter.ToString()));
                }
            }
        }
    }
}

var api = new RestClient("http://api.example.com")
{
    RequestQueryParamSerializer = new XmlRequestQueryParamSerializer()
}.For<ISomeApi>();
```


Controlling the Requests
------------------------

RestEase provides two ways for you to manipulate how exactly requests are made, before you need to resort to [Customizing RestEase](#customizing-restease).

### `RequestModifier`

The first is a `RestClient.For<T>` overload which lets you specify a delegate which is invoked whenever a request is made.
This allows you to inspect and alter the request in any way you want: changing the content, changing the headers, make your own requests in the meantime, etc.

For example, if you need to refresh an oAuth access token occasionally (using the [ADAL](https://msdn.microsoft.com/en-us/library/azure/jj573266.aspx) library as an example):

```csharp
public interface IMyRestService
{
    [Get("getPublicInfo")]
    Task<Foobar> SomePublicMethodAsync();

    [Get("secretStuff")]
    [Header("Authorization", "Bearer")]
    Task<Location> GetLocationOfRebelBaseAsync();
}

AuthenticationContext context = new AuthenticationContext(...);
IGitHubApi api = RestClient.For<IGitHubApi>("http://api.github.com", async (request, cancellationToken) =>
{
    // See if the request has an authorize header
    var auth = request.Headers.Authorization;
    if (auth != null)
    {
        // The AquireTokenAsync call will prompt with a UI if necessary
        // Or otherwise silently use a refresh token to return a valid access token 
        var token = await context.AcquireTokenAsync("http://my.service.uri/app", "clientId", new Uri("callback://complete")).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, token);
  }
});

```

### Custom `HttpClient`

The second is a `RestClient.For<T>` overload which lets you specify a custom `HttpClient` to use.
This lets you customize the `HttpClient`, e.g. to set the request timeout.
It also lets you specify a custom `HttpMessageHandler` subclass, which allows you to control all sorts of things.

For example, if you wanted to 1) adjust the request timeout, and 2) allow invalid certificates (although the same approach would apply if you wanted to customize how certificates are validated), you could do something like this. Note that `WebRequestHandler` is a `HttpMessageHandler` subclass which allows you to specify things like `ServerCertificateValidationCallback`.

```csharp
public class CustomHttpClientHandler : WebRequestHandler
{
    // Let's log all of our requests!
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public CustomHttpClientHandler()
    {
        // Allow any cert, valid or invalid
        this.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (logger.IsTraceEnabled)
        {
            var response = await base.SendAsync(request, cancellationToken);
            logger.Trace((await response.Content.ReadAsStringAsync()).Trim());
            return response;
        }
        else
        {
            return await base.SendAsync(request, cancellationToken);
        }
    }
}

var httpClient = new HttpClient(new CustomHttpClientHandler())
{
    BaseAddress = new Uri("https://secure-api.example.com"),
    Timeout = TimeSpan.FromSeconds(3), // Very slow to respond, this server
};

ISomeApi api = RestClient.For<ISomeApi>(httpClient);
```


Customizing RestEase
--------------------

You've already seen how to [specify custom Serializers and Deserializers](#controlling-serialization-and-deserialization), and [control requests](#controlling-the-requests).

RestEase has been written in a way which makes it very easy to customize exactly how it works.
In order to describe this, I'm first going to have to outline its architecture.

Given an API like:

```csharp
public interface ISomeApi
{
    [Get("users/{userId}")]
    Task GetUserAsync([Path] string userId);
}
```

Calling `RestClient.For<ISomeApi>(...)` will cause a class like this to be generated:

```csharp
namespace RestEase.AutoGenerated
{
    public class ISomeApi
    {
        private readonly IRequester requester;

        public ISomeApi(IRequester requester)
        {
            this.requester = requester;
        }

        public Task GetUserAsync(string userId)
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "users/{userId}");
            requestInfo.AddPathParameter<string>("userId", userId);
            return this.requester.RequestVoidAsync(requestInfo);
        }
    }
}
```

Now, you cannot customize what this generated class looks like, but you can see it doesn't actually do very much: it just builds up a `RequestInfo` object, then sends it off to the [`IRequester`](https://github.com/canton7/RestEase/blob/master/src/RestEase/IRequester.cs) (which does all of the hard work).
What you *can* do however is to provide your own [`IRequester`](https://github.com/canton7/RestEase/blob/master/src/RestEase/IRequester.cs) implementation, and pass that to an appropriate overload of `RestClient.For<T>`.
In fact, the default implementation of [`IRequester`](https://github.com/canton7/RestEase/blob/master/src/RestEase/IRequester.cs), [`Requester`](https://github.com/canton7/RestEase/blob/master/src/RestEase/Implementation/Requester.cs), has been carefully written so that it's easy to extend: each little bit of functionality is broken out into its own virtual method, so it's easy to replace just the behaviour you need.

Have a read through [`Requester`](https://github.com/canton7/RestEase/blob/master/src/RestEase/Implementation/Requester.cs), figure out what you want to change, subclass it, and provide an instance of that subclass to `RestClient.For<T>`. 


Interface Accessibility
-----------------------

Since RestEase generates an interface implementation in a separate assembly, the interface ideally needs to be public.

If you don't want to do this, you'll need to mark RestEase as being a 'friend' assembly, which allows RestEase to see your internal types.
Add the following line to your `AssemblyInfo.cs`:

```
[assembly: InternalsVisibleTo(RestEase.RestClient.FactoryAssemblyName)]
```


Using Generic Interfaces
------------------------

When using something like ASP.NET Web API, it's a fairly common pattern to have a whole stack of CRUD REST services. RestEase supports these, allowing you to define a single API interface with a generic type:

```csharp
public interface IReallyExcitingCrudApi<T, TKey>
{
    [Post("")]
    Task<T> Create([Body] T paylod);

    [Get("")]
    Task<List<T>> ReadAll();

    [Get("{key}")]
    Task<T> ReadOne(TKey key);

    [Put("{key}")]
    Task Update(TKey key, [Body]T payload);

    [Delete("{key}")]
    Task Delete(TKey key);
}
```

Which can be used like this:

```csharp
// The "/users" part here is kind of important if you want it to work for more 
// than one type (unless you have a different domain for each type)
var api = RestClient.For<IReallyExcitingCrudApi<User, string>>("http://api.example.com/users"); 
```

Interface Inheritance
---------------------

You're allowed to use interface inheritance to share common properties and methods between different APIs.
However, you are not allowed to put any attributes (`[Header]` or `[AllowAnyStatusCode]`) onto the child interfaces being inherited, just onto the parent-most interface.

For example:

```csharp
public interface IAuthenticatedEndpoint
{
    [Header("X-Api-Token")]
    string ApiToken { get; set; }
    [Header("X-Api-Username")]
    string ApiUsername { get; set; }
}

public interface IDevicesEndpoint : IAuthenticatedEndpoint
{
    [Get("/devices")]
    Task<IList<Device>> GetAllDevices([QueryMap] IDictionary<string, string> filters);
}

public interface IUsersEndpoint : IAuthenticatedEndpoint
{
    [Get("/user")]
    Task<User> FetchUserAsync(int userid)
}
```

FAQs
----

### I want to use Basic Authentication

Something like this...

```csharp
public interface ISomeApi
{
    [Header("Authorization")]
    AuthenticationHeaderValue Authorization { get; set; }

    [Get("foo")]
    Task DoSomethingAsync();
}

ISomeApi api = RestClient.For<ISomeApi>("http://api.example.com");
var value = Convert.ToBase64String(Encoding.ASCII.GetBytes("username:password1234"));
api.Authorization = new AuthenticationHeaderValue("Basic", value);

await api.DoSomethingAsync();
```

### I need to request an absolute path

Sometimes your API responses will contain absolute URLs, for example a "next page" link.
Therefore you'll want a way to request a resource using an absolute URL which overrides the base URL you specified.

Thankfully this is easy: if you give an absolute URL to e.g. `[Get("http://api.example.com/foo")]`, then the base URL will be ignored.

```csharp
public interface ISomeApi
{
    [Get("users")]
    Task<UsersResponse> FetchUsersAsync();

    [Get("{url}")]
    Task<UsersResponse> FetchUsersByUrlAsync([Path] string url);
}

ISomeApi api = RestClient.For<ISomeApi("http://api.example.com");

var firstPage = await api.FetchUsersAsync();
// Actually put decent logic here...
var secondPage = await api.FetchUsersByUrlAsync(firstPage.NextPage);
```

### I may get responses in both XML and JSON, and want to deserialize both

Occasionally you get an API which can return both JSON and XML (apparently...).
In this case, you'll want to auto-detect what sort of response you got, and deserialize with an appropriate deserializer.

To do this, use a custom deserializer, which can do this detection.

```csharp
public class HybridResponseDeserializer : IResponseDeserializer
{
    private T DeserializeXml<T>(string content)
    {
        // Consider caching generated XmlSerializers
        var serializer = new XmlSerializer(typeof(T));

        using (var stringReader = new StringReader(content))
        {
            return (T)serializer.Deserialize(stringReader);
        }
    }

    private T DeserializeJson<T>(string content)
    {
        return JsonConvert.Deserialize<T>(content);
    }

    public T Deserialize<T>(string content, HttpResponseMessage response)
    {
        switch (response.Content.Headers.ContentType.MediaType)
        {
            case "application/json":
                return this.DeserializeJson<T>(content);
            case "application/xml":
                return this.DeserializeXml<T>(content);
        }

        throw new ArgumentException("Response was not JSON or XML");
    }
}

var api = RestClient.For<ISomeApi>("http://api.example.com", new HybridResponseDeserializer());
```

### I want to upload a file

Let's assume you want to upload a file (from a stream), setting its name and content-type manually (skip these bits of not).
There are a couple of ways of doing this, depending on your needs:

```csharp
public interface ISomeApi
{
    [Header("Content-Disposition", "form-data; filename=\"somefile.txt\"")]
    [Header("Content-Type", "text/plain")]
    [Post("upload")]
    Task UploadFileVersionOneAsync([Body] Stream file);

    [Post("upload")]
    // You can use strings instead of strongly-typed header values, if you want
    Task UploadFileVersionTwoAsync(
        [Header("Content-Disposition")] ContentDispositionHeaderValue contentDisposition,
        [Header("Content-Type")] MediaTypeHeaderValue contentType,
        [Body] Stream file);

    [Post("upload")]
    Task UploadFileVersionThreeAsync([Body] HttpContent content);
}

ISomeApi api = RestClient.For<ISomeApi>("http://api.example.com");

// Version one (constant headers)
using (var fileStream = File.OpenRead("somefile.txt"))
{
    await api.UploadFileVersionOneAsync(fileStream);
}

// Version two (variable headers)
using (var fileStream = File.OpenRead("somefile.txt"))
{
    var contentDisposition = new ContentDispositionHeaderValue("form-header") { FileName = "\"somefile.txt\"" };
    var contentType = new MediaTypeHeaderValue("text/plain");
    await api.UploadFileVersionTwoAsync(contentDisposition, contentType, fileStream);
}

// Version three (precise control over HttpContent)
using (var fileStream = File.OpenRead("somefile.txt"))
{
    var fileContent = new StreamContent(fileStream);
    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-header") { FileName = "\"somefile.txt\"" };
    fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
    await api.UploadFileVersionThreeAsync(fileContent);
}
```

Obviously, set the headers you need - don't just copy me blindly.

Comparison to Refit
-------------------

RestEase is very heavily inspired by [Paul Betts' Refit](https://github.com/paulcbetts/refit).
Refit is a fantastic library, and in my opinion does a lot of things very right.
It was the first C# REST client library that I actually enjoyed using.

I wrote RestEase for two reasons: 1) there were a couple of things about Refit which I didn't like, and 2) I thought it would be fun.

Here's a brief summary of pros/cons, compared to Refit:

### Pros

 - No autogenerated `RefitStubs.cs`
 - Supports `CancellationTokens` for Task-based methods
 - Supports method overloading
 - Supports property-defined headers
 - Better support for API calls which are expected to fail: `[AllowAnyStatusCode]` and `Response<T>`
 - Easier to customize:
   - Can specify custom response deserializer
   - Can specify custom request serializer
   - Can customize almost every aspect of setting up and creating the request (through implementing `IRequester`)
 - Supports `[QueryMap]`
 - Supports custom query parameter serialization
 - Supports arrays of query parameters (and body parameters when serializing a body parameter as UrlEncoded)
 - Supports `IDictionary<TKey, TValue>` as well as `IDictionary` types when serializing a body parameter as UrlEncoded. This allows e.g. `ExpandoObject` to be used here

### Cons

 - Interfaces need to be public, or you need to add `[assembly: InternalsVisibleTo(RestClient.FactoryAssemblyName)]` to your `AssemblyInfo.cs`
 - Only supports .NET Framework
 - No `IObservable` support
 - Slightly more work done at runtime (but not very much more)
