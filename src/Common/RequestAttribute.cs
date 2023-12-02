using System;
using System.Net.Http;

namespace RestEase
{
    /// <summary>
    /// Base class for all request attributes
    /// </summary>
    public abstract class RequestAttributeBase : Attribute
    {
        /// <summary>
        /// Gets the HTTP method to use (Get/Post/etc)
        /// </summary>
        public HttpMethod Method { get; }

        /// <summary>
        /// Gets or sets the path to request. This is relative to the base path configured when RestService.For was called, and can contain placeholders
        /// </summary>
        public string? Path { get; set; }

        internal RequestAttributeBase(HttpMethod method)
        {
            this.Method = method;
        }

        internal RequestAttributeBase(HttpMethod method, string path)
        {
            this.Method = method;
            this.Path = path;
        }
    }

    /// <summary>
    /// Attribute for custom HTTP methods which aren't represented by other subclasses of RequestAttributeBase
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class RequestAttribute : RequestAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestAttribute"/> class, with the given HttpMethod.
        /// </summary>
        /// <remarks>
        /// Use this if there isn't a <see cref="RequestAttribute"/> subclass for the HTTP method you want to use
        /// </remarks>
        /// <param name="httpMethod">HTTP Method to use, e.g. "PATCH"</param>
        public RequestAttribute(string httpMethod)
            : base(new HttpMethod(httpMethod))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestAttribute"/> class, with the given HttpMethod and relative path.
        /// </summary>
        /// <remarks>
        /// Use this if there isn't a <see cref="RequestAttribute"/> subclass for the HTTP method you want to use
        /// </remarks>
        /// <param name="httpMethod">HTTP Method to use, e.g. "PATCH"</param>
        /// <param name="path">Relative path to use</param>
        public RequestAttribute(string httpMethod, string path)
            : base(new HttpMethod(httpMethod), path)
        {
        }
    }

    /// <summary>
    /// Delete request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class DeleteAttribute : RequestAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteAttribute"/> class
        /// </summary>
        public DeleteAttribute() : base(HttpMethod.Delete) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public DeleteAttribute(string path) : base(HttpMethod.Delete, path) { }
    }

    /// <summary>
    /// Get request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class GetAttribute : RequestAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetAttribute"/> class
        /// </summary>
        public GetAttribute() : base(HttpMethod.Get) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public GetAttribute(string path) : base(HttpMethod.Get, path) { }
    }

    /// <summary>
    /// Head request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HeadAttribute : RequestAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeadAttribute"/> class
        /// </summary>
        public HeadAttribute() : base(HttpMethod.Head) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeadAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public HeadAttribute(string path) : base(HttpMethod.Head, path) { }
    }

    /// <summary>
    /// Options request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OptionsAttribute : RequestAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsAttribute"/> class
        /// </summary>
        public OptionsAttribute() : base(HttpMethod.Options) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public OptionsAttribute(string path) : base(HttpMethod.Options, path) { }
    }

    /// <summary>
    /// Post request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PostAttribute : RequestAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostAttribute"/> class
        /// </summary>
        public PostAttribute() : base(HttpMethod.Post) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public PostAttribute(string path) : base(HttpMethod.Post, path) { }
    }

    /// <summary>
    /// Put request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PutAttribute : RequestAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PutAttribute"/> class
        /// </summary>
        public PutAttribute() : base(HttpMethod.Put) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PutAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public PutAttribute(string path) : base(HttpMethod.Put, path) { }
    }

    /// <summary>
    /// Trace request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TraceAttribute : RequestAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceAttribute"/> class
        /// </summary>
        public TraceAttribute() : base(HttpMethod.Trace) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public TraceAttribute(string path) : base(HttpMethod.Trace, path) { }
    }

    /// <summary>
    /// Patch request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PatchAttribute : RequestAttributeBase
    {
        /// <summary>
        /// Gets a static instance of <see cref="HttpMethod"/> corresponding to a PATCH request
        /// </summary>
        public static HttpMethod PatchMethod { get; } = new HttpMethod("PATCH");

        /// <summary>
        /// Initializes a new instance of the <see cref="PatchAttribute"/> class
        /// </summary>
        public PatchAttribute() : base(PatchMethod) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatchAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public PatchAttribute(string path) : base(PatchMethod, path) { }
    }
}
