using System;
using System.Net.Http;

namespace RestEase
{
    /// <summary>
    /// Base class for all request attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class RequestAttribute : Attribute
    {
        /// <summary>
        /// Gets the HTTP method to use (Get/Post/etc)
        /// </summary>
        public HttpMethod Method { get; private set; }

        /// <summary>
        /// Gets or sets the path to request. This is relative to the base path configured when RestService.For was called, and can contain placeholders
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="RequestAttribute"/> class, with the given HttpMethod
        /// </summary>
        /// <param name="method">HttpMethod to use</param>
        public RequestAttribute(HttpMethod method)
        {
            this.Method = method;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="RequestAttribute"/> class, with the given HttpMethod and relative path
        /// </summary>
        /// <param name="method">HttpMethod to use</param>
        /// <param name="path">Relative path to use</param>
        public RequestAttribute(HttpMethod method, string path) : this(method)
        {
            this.Path = path;
        }
    }

    /// <summary>
    /// Delete request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class DeleteAttribute : RequestAttribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="DeleteAttribute"/> class
        /// </summary>
        public DeleteAttribute() : base(HttpMethod.Delete) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="DeleteAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public DeleteAttribute(string path) : base(HttpMethod.Delete, path) { }
    }

    /// <summary>
    /// Get request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class GetAttribute : RequestAttribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="GetAttribute"/> class
        /// </summary>
        public GetAttribute() : base(HttpMethod.Get) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="GetAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public GetAttribute(string path) : base(HttpMethod.Get, path) { }
    }

    /// <summary>
    /// Head request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HeadAttribute : RequestAttribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="HeadAttribute"/> class
        /// </summary>
        public HeadAttribute() : base(HttpMethod.Head) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="HeadAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public HeadAttribute(string path) : base(HttpMethod.Head, path) { }
    }

    /// <summary>
    /// Options request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OptionsAttribute : RequestAttribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="OptionsAttribute"/> class
        /// </summary>
        public OptionsAttribute() : base(HttpMethod.Options) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="OptionsAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public OptionsAttribute(string path) : base(HttpMethod.Options, path) { }
    }

    /// <summary>
    /// Post request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PostAttribute : RequestAttribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="PostAttribute"/> class
        /// </summary>
        public PostAttribute() : base(HttpMethod.Post) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="PostAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public PostAttribute(string path) : base(HttpMethod.Post, path) { }
    }

    /// <summary>
    /// Put request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PutAttribute : RequestAttribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="PutAttribute"/> class
        /// </summary>
        public PutAttribute() : base(HttpMethod.Put) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="PutAttribute"/> class, with the given relativ epath
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public PutAttribute(string path) : base(HttpMethod.Put, path) { }
    }

    /// <summary>
    /// Trace request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TraceAttribute : RequestAttribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="TraceAttribute"/> class
        /// </summary>
        public TraceAttribute() : base(HttpMethod.Trace) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="TraceAttribute"/> class, with the given relative path
        /// </summary>
        /// <param name="path">Relative path to use</param>
        public TraceAttribute(string path) : base(HttpMethod.Trace, path) { }
    }
}
