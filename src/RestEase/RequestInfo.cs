using System;
using System.Collections.Generic;
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

        private readonly List<RequestParameter> parameters = new List<RequestParameter>();
        public IReadOnlyList<RequestParameter> Parameters
        {
            get { return this.parameters.AsReadOnly(); }
        }

        public RequestInfo(HttpMethod method, string path, CancellationToken cancellationToken)
        {
            this.Method = method;
            this.Path = path;
            this.CancellationToken = cancellationToken;
        }

        public void AddParameter(string name, string value)
        {
            this.parameters.Add(new RequestParameter(name, value));
        }
    }
}
