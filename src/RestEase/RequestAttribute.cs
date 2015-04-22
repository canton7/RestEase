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
    public sealed class GetAttribute : RequestAttribute
    {
        public GetAttribute() : base(HttpMethod.Get)
        { }

        public GetAttribute(string path) : base(HttpMethod.Get, path)
        { }
    }
}
