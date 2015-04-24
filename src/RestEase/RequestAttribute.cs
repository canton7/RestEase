using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Base class for all request attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple =false)]
    public class RequestAttribute : Attribute
    {
        /// <summary>
        /// HTTP method to use (Get/Post/etc)
        /// </summary>
        public HttpMethod Method { get; private set; }

        /// <summary>
        /// Path to request. This is relative to the base path configured when RestService.For was called, and can contain placeholders
        /// </summary>
        public string Path { get; set; }

        public RequestAttribute(HttpMethod method)
        {
            this.Method = method;
        }

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
        public DeleteAttribute() : base(HttpMethod.Delete) { }
        public DeleteAttribute(string path) : base(HttpMethod.Delete, path) { }
    }

    /// <summary>
    /// Get request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class GetAttribute : RequestAttribute
    {
        public GetAttribute() : base(HttpMethod.Get) { }
        public GetAttribute(string path) : base(HttpMethod.Get, path) { }
    }

    /// <summary>
    /// Head request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HeadAttribute : RequestAttribute
    {
        public HeadAttribute() : base(HttpMethod.Head) { }
        public HeadAttribute(string path) : base(HttpMethod.Head, path) { }
    }

    /// <summary>
    /// Options request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OptionsAttribute : RequestAttribute
    {
        public OptionsAttribute() : base(HttpMethod.Options) { }
        public OptionsAttribute(string path) : base(HttpMethod.Options, path) { }
    }

    /// <summary>
    /// Post request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PostAttribute : RequestAttribute
    {
        public PostAttribute() : base(HttpMethod.Post) { }
        public PostAttribute(string path) : base(HttpMethod.Post, path) { }
    }

    /// <summary>
    /// Put request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PutAttribute : RequestAttribute
    {
        public PutAttribute() : base(HttpMethod.Put) { }
        public PutAttribute(string path) : base(HttpMethod.Put, path) { }
    }

    /// <summary>
    /// Trace request
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TraceAttribute : RequestAttribute
    {
        public TraceAttribute() : base(HttpMethod.Trace) { }
        public TraceAttribute(string path) : base(HttpMethod.Trace, path) { }
    }
}
