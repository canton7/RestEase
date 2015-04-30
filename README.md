RestEase
========

RestEase is a little type-safe REST API client library, which aims to make interacting with remote REST endpoints easy, without adding unnecessary compexity.

Almost every aspect of RestEase can be overridden and customized, leading to a large level of flexibility.

To use it, you define an interface which represents the endpoint you wish to communicate with (more on that in a bit), where methods on that interface correspond to requests that can be made on it.
RestEase will then generate an implementation of that interface for you, and by calling the methods you defined, the appropriate requests will be made.

Quick Start
-----------

To start, first create an interface which represents the endpoint you wish to make requests to.
Please note that it does have to be public, or you must add RestEase as a friend assembly, see TODO BELOW.

```csharp
// Define an interface representing the API
public interface IGithubApi
{
    // All interface methods must return a Task or Task<T>. We'll discuss what sort of T in more detail below.

    // The [Get] attribute marks this method as a GET request
    // The "users" is a relative path the a base URL, which we'll provide later
    [Get("users")]
    Task<List<User>> GetUsersAsync();
}

// Create an implementation of that interface
// We'll pass in the base URL for the API
IGithubApi api = RestClient.For<IGithubApi>("http://api.github.com");

// Now we can simply call methods on it
List<User> users = await api.GetUsersAsync();
```


Request Types
-------------

See the `[Get("path")]` attribute used above?
That's how you mark that method as being a GET request.
There are a number of other attributes you can use here - in fact, there's one for each type of request: `[Post("path")]`, `[Delete("path")]`, etc. Use whichever one you need to.


Return Types
------------

Your interface methods may return one of the following types:

 - `Task`: This method does not return any data, but the task will complete when the request has completed
 - `Task<T>` (where `T` is not one of the types listed below): This method will deserialize the response into an object of type `T`, using its configured deserializer, see TODO BELOW
 - `Task<string>`: This method returns the raw response, as a string
 - `Task<HttpResponseMessage>`: This method returns the raw `HttpResponseMessage` resulting from the request. It does not do any deserialiation
 - `Task<Response<T>>`: This method returns a `Response<T>`. A `Response<T>` contains both the deserialied response (of type `T`), but also the `HttpResponseMessage`. Use this when you want to have both the deserialized response, and access to things like the response headers

Non-async methods are not supported (use `.Wait()` or `Result` as appropriate if you do want to make your request synchronous).


Query Parameters
----------------

It is very common to want to include query parameters in your request (e.g. `/foo?key=value`), and RestEase makes this easy.
Any parameters to a method which are:

 - Decorated with the `[QueryParam]` attribute, or
 - Not decorated at all

will be interpreted as query parameters.

The name of the parameter will be used as the key, unless an argument is passed to `[QueryParam("key")]`, in which case that will be used instead.

For example:

```csharp
public interface IGithubApi
{
	[Get("user")]
    Task<User> FetchUserAsync(int userid);

    // Is the same as

    [Get("user")]
    Task<User> FetchUserAsync([QueryParam] int userid);

    // Is the same as

    [Get("user")]
    Task<User> FetchUserAsync([QueryParam("userid")] int userId);
}

IGithubApi api = RestClient.For<IGithubApi>("http://api.github.com");

// Requests http://api.github.com/user?userId=3
await api.FetchUserAsync(3);
```

Constant query parameters can just be specified in the path:

```csharp
public interface ISomeApi
{
    [Get("users?userid=3")]
    Task<User> GetUserIdThreeAsync();
}
```

You can have duplicate keys if you want:

```csharp
public interface ISomeApi
{
    [Get("search")]
    Task<SearchResult> SearchAsync([QueryParam("filter")] string filter1, [QueryParam("filter")] string filter2);
}

ISomeApi api = RestClient.For<ISomeApi>("http://someendpoint.com");

// Requests http://somenedpoint.com/search?filter=foo&filter=bar
await api.SearchAsync("foo", "bar");
```

You can also have an array of query parameters:

```csharp
public interface ISomeApi
{
	// You can use IEnumerable<T>, or any type which implements IEnumerable<T>

    [Get("search")]
    Task<SearchResult> SearchAsync([QueryParam("filter")] IEnumerable<string> filters);
}

ISomeApi api = RestClient.For<ISomeApi>("http://someendpoint.com");

// Requests http://somenedpint.com/search?filter=foo&filter=bar&filter=baz
await api.SearchAsync(new[] { "foo", "bar", "baz" });
```


Path Parameters
---------------

Sometimes you also want to be able to control some parts of the path itself, rather than just the query parameters.
This is done using placeholders in the path, and corresponding method parameters decorated with `[PathParam]`.

For example:

```csharp
public interface ISomeApi
{
    [Get("user/{userId}")]
    Task<User> FetchUserAsync([PathParam] string userId);
}

ISomeApi api = RestClient.For<ISomeApi>("http://example.com");

// Requests http://example.com/user/fred
await api.FetchUserAsync("fred");
```

As with `[QueryParam]`, the name of the placeholder to substitute is determined by the name of the parameter.
If you want to override this, you can pass an argument to `[QueryParam("placeholder")]`, e.g.:

```csharp
public interface ISomeApi
{
    [Get("user/{userId}")]
    Task<User> FetchUserAsync([PathParam("userId")] string idOfTheUser);
}
```

Every placeholder must have a corresponding parameter, and every parameter must relate to a placeholder.


Headers
-------

Specifying headers is actually a surprisingly large topic, and is broken down into several levels: interface, method, and parameter.

 - Interface headers are constant, and apply to all method in that interface.
 - Method headers are constant, and apply to just that method, and override the interface headers.
 - Parameter headers are dynamic: that is, you can specify their value per-request. They apply to a single method, and override the method headers.

### Interface Headers

Interface headers are specified using the `[Header("Name: Value")]` attribute, applied to the interface:

```csharp
[Header("User-Agent: RestEase")]
public interface ISomeApi
{
   [Get("users")]
   Task<User> FetchUserAsync(int userId);
}
```

You can specify multiple headers in this way if you wish.

### Method Headers

Method headers are specified using the `[Header("Name: Value")]` attribute, but this time it's applied to a method within the interface.
Headers specified in this way are added to any interface headers, unless a header is redefined, in which case it is replaced.

Example:

```csharp

```