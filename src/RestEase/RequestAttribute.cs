using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple =false)]
    public class RequestAttribute : Attribute
    {
        public HttpMethod Method { get; private set; }
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

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class DeleteAttribute : RequestAttribute
    {
        public DeleteAttribute() : base(HttpMethod.Delete) { }
        public DeleteAttribute(string path) : base(HttpMethod.Delete, path) { }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class GetAttribute : RequestAttribute
    {
        public GetAttribute() : base(HttpMethod.Get) { }
        public GetAttribute(string path) : base(HttpMethod.Get, path) { }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HeadAttribute : RequestAttribute
    {
        public HeadAttribute() : base(HttpMethod.Head) { }
        public HeadAttribute(string path) : base(HttpMethod.Head, path) { }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OptionsAttribute : RequestAttribute
    {
        public OptionsAttribute() : base(HttpMethod.Options) { }
        public OptionsAttribute(string path) : base(HttpMethod.Options, path) { }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PostAttribute : RequestAttribute
    {
        public PostAttribute() : base(HttpMethod.Post) { }
        public PostAttribute(string path) : base(HttpMethod.Post, path) { }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PutAttribute : RequestAttribute
    {
        public PutAttribute() : base(HttpMethod.Put) { }
        public PutAttribute(string path) : base(HttpMethod.Put, path) { }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TraceAttribute : RequestAttribute
    {
        public TraceAttribute() : base(HttpMethod.Trace) { }
        public TraceAttribute(string path) : base(HttpMethod.Trace, path) { }
    }
}
