using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase
{
    public class RequestParameter
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public RequestParameter(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class RequestInfo
    {
        public HttpMethod Method { get; private set; }
        public string Path { get; private set; }
        public CancellationToken CancellationToken { get; private set; }
        public List<KeyValuePair<string, string>> QueryParams { get; private set; }
        public List<KeyValuePair<string, string>> PathParams { get; private set; }

        public RequestInfo(HttpMethod method, string path, CancellationToken cancellationToken)
        {
            this.Method = method;
            this.Path = path;
            this.CancellationToken = cancellationToken;
            this.QueryParams = new List<KeyValuePair<string, string>>();
            this.PathParams = new List<KeyValuePair<string, string>>();
        }

        public void AddQueryParameter(string name, string value)
        {
            this.QueryParams.Add(new KeyValuePair<string, string>(name, value));
        }

        public void AddPathParameter(string name, string value)
        {
            this.PathParams.Add(new KeyValuePair<string, string>(name, value));
        }
    }
}
