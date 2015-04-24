using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    public class BodyParameterInfo
    {
        public BodySerializationMethod SerializationMethod { get; private set; }
        public object Value { get; private set; }

        public BodyParameterInfo(BodySerializationMethod serializationMethod, object value)
        {
            this.SerializationMethod = serializationMethod;
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
        public List<string> ClassHeaders { get; private set; }
        public List<string> MethodHeaders { get; private set; }
        public List<KeyValuePair<string, string>> HeaderParams { get; private set; }
        public BodyParameterInfo BodyParameterInfo { get; private set; }

        public RequestInfo(HttpMethod method, string path, CancellationToken cancellationToken)
        {
            this.Method = method;
            this.Path = path;
            this.CancellationToken = cancellationToken;

            this.QueryParams = new List<KeyValuePair<string, string>>();
            this.PathParams = new List<KeyValuePair<string, string>>();
            this.ClassHeaders = new List<string>();
            this.MethodHeaders = new List<string>();
            this.HeaderParams = new List<KeyValuePair<string, string>>();
        }

        public void AddQueryParameter(string name, string value)
        {
            this.QueryParams.Add(new KeyValuePair<string, string>(name, value));
        }

        public void AddPathParameter(string name, string value)
        {
            this.PathParams.Add(new KeyValuePair<string, string>(name, value));
        }

        public void AddClassHeader(string header)
        {
            this.ClassHeaders.Add(header);
        }

        public void AddMethodHeader(string header)
        {
            this.MethodHeaders.Add(header);
        }

        public void AddHeaderParameter(string name, string value)
        {
            this.HeaderParams.Add(new KeyValuePair<string, string>(name, value));
        }

        public void SetBodyParameterInfo(BodySerializationMethod serializationMethod, object value)
        {
            this.BodyParameterInfo = new BodyParameterInfo(serializationMethod, value);
        }
    }
}
