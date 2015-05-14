using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace RestEase.Implementation
{
    internal class ParameterGrouping
    {
        public List<IndexedParameter<PathAttribute>> PathParameters { get; private set; }
        public List<IndexedParameter<QueryAttribute>> QueryParameters { get; private set; }
        public List<IndexedParameter<HeaderAttribute>> HeaderParameters { get; private set; }
        public List<IndexedParameter> PlainParameters { get; private set; }
        public IndexedParameter<BodyAttribute>? Body { get; private set; }
        public IndexedParameter? CancellationToken { get; private set; }

        public ParameterGrouping(IEnumerable<ParameterInfo> parameters, string methodName)
        {
            this.PathParameters = new List<IndexedParameter<PathAttribute>>();
            this.QueryParameters = new List<IndexedParameter<QueryAttribute>>();
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
                        throw new ImplementationCreationException(String.Format("Found more than one parameter with a [Body] attribute for method {0}", methodName));
                    this.Body = new IndexedParameter<BodyAttribute>(parameter.Index, parameter.Parameter, bodyAttribute);
                    continue;
                }

                var queryParamAttribute = parameter.Parameter.GetCustomAttribute<QueryAttribute>();
                if (queryParamAttribute != null)
                {
                    this.QueryParameters.Add(new IndexedParameter<QueryAttribute>(parameter.Index, parameter.Parameter, queryParamAttribute));
                    continue;
                }

                var pathParamAttribute = parameter.Parameter.GetCustomAttribute<PathAttribute>();
                if (pathParamAttribute != null)
                {
                    this.PathParameters.Add(new IndexedParameter<PathAttribute>(parameter.Index, parameter.Parameter, pathParamAttribute));
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
