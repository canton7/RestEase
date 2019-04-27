using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using RestEase.Platform;

namespace RestEase.Implementation
{
    internal class ParameterGrouping
    {
        public List<IndexedParameter<PathAttribute>> PathParameters { get; private set; }
        public List<IndexedParameter<QueryAttribute>> QueryParameters { get; private set; }
        public List<IndexedParameter<RequestPropertyAttribute>> HttpRequestMessageProperties { get; private set; }
        public IndexedParameter<RawQueryStringAttribute>? RawQueryString { get; private set; }
        public List<IndexedParameter<QueryMapAttribute>> QueryMaps { get; private set; }
        public List<IndexedParameter<HeaderAttribute>> HeaderParameters { get; private set; }
        public List<IndexedParameter> PlainParameters { get; private set; }
        public IndexedParameter<BodyAttribute>? Body { get; private set; }
        public IndexedParameter? CancellationToken { get; private set; }

        public ParameterGrouping(IEnumerable<ParameterInfo> parameters, string methodName)
        {
            this.PathParameters = new List<IndexedParameter<PathAttribute>>();
            this.QueryParameters = new List<IndexedParameter<QueryAttribute>>();
            this.HttpRequestMessageProperties = new List<IndexedParameter<RequestPropertyAttribute>>();
            this.QueryMaps = new List<IndexedParameter<QueryMapAttribute>>();
            this.HeaderParameters = new List<IndexedParameter<HeaderAttribute>>();
            this.PlainParameters = new List<IndexedParameter>();

            // Index 0 is 'this'
            var indexedParameters = parameters.Select((x, i) => new { Index = i + 1, Parameter = x });
            foreach (var parameter in indexedParameters)
            {
                if (parameter.Parameter.ParameterType == typeof(CancellationToken))
                {
                    if (this.CancellationToken.HasValue)
                        throw new ImplementationCreationException(String.Format("Found more than one parameter of type CancellationToken for method {0}", methodName));
                    this.CancellationToken = new IndexedParameter(parameter.Index, parameter.Parameter);
                    continue;
                }

                var bodyAttribute = parameter.Parameter.GetCustomAttribute<BodyAttribute>();
                if (bodyAttribute != null)
                {
                    if (this.Body.HasValue)
                        throw new ImplementationCreationException(String.Format("Method '{0}': found more than one parameter with a [Body] attribute", methodName));
                    this.Body = new IndexedParameter<BodyAttribute>(parameter.Index, parameter.Parameter, bodyAttribute);
                    continue;
                }

                var queryMapAttribute = parameter.Parameter.GetCustomAttribute<QueryMapAttribute>();
                if (queryMapAttribute != null)
                {
                    this.QueryMaps.Add(new IndexedParameter<QueryMapAttribute>(parameter.Index, parameter.Parameter, queryMapAttribute));
                    continue;
                }

                var queryParamAttribute = parameter.Parameter.GetCustomAttribute<QueryAttribute>();
                if (queryParamAttribute != null)
                {
                    this.QueryParameters.Add(new IndexedParameter<QueryAttribute>(parameter.Index, parameter.Parameter, queryParamAttribute));
                    continue;
                }

                var rawQueryStringAttribute = parameter.Parameter.GetCustomAttribute<RawQueryStringAttribute>();
                if (rawQueryStringAttribute != null)
                {
                    if (this.RawQueryString.HasValue)
                        throw new ImplementationCreationException(String.Format("Method '{0}': found more than one parameter with a [RawQueryString] attribute", methodName));
                    this.RawQueryString = new IndexedParameter<RawQueryStringAttribute>(parameter.Index, parameter.Parameter, rawQueryStringAttribute);
                    continue;
                }

                var pathParamAttribute = parameter.Parameter.GetCustomAttribute<PathAttribute>();
                if (pathParamAttribute != null)
                {
                    this.PathParameters.Add(new IndexedParameter<PathAttribute>(parameter.Index, parameter.Parameter, pathParamAttribute));
                    continue;
                }

                var httpRequestMessagePropertyParamAttribute = parameter.Parameter.GetCustomAttribute<RequestPropertyAttribute>();
                if (httpRequestMessagePropertyParamAttribute != null)
                {
                    this.HttpRequestMessageProperties.Add(new IndexedParameter<RequestPropertyAttribute>(parameter.Index, parameter.Parameter, httpRequestMessagePropertyParamAttribute));
                    continue;
                }

                var headerAttribute = parameter.Parameter.GetCustomAttribute<HeaderAttribute>();
                if (headerAttribute != null)
                {
                    this.HeaderParameters.Add(new IndexedParameter<HeaderAttribute>(parameter.Index, parameter.Parameter, headerAttribute));
                    continue;
                }

                // Anything left? It's a query parameter
                this.PlainParameters.Add(new IndexedParameter(parameter.Index, parameter.Parameter));
            }
        }
    }
}
