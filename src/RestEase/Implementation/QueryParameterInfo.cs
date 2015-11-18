using System;
using System.Collections.Generic;
using System.Linq;

namespace RestEase.Implementation
{
    /// <summary>
    /// Class containing information about a desired query parameter
    /// </summary>
    public abstract class QueryParameterInfo
    {
        /// <summary>
        /// Gets or sets the method to use to serialize the query parameter
        /// </summary>
        public QuerySerialializationMethod SerializationMethod { get; protected set; }

        /// <summary>
        /// Serialize the (typed) value into a collection of name -> value pairs using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <returns>Serialized value</returns>
        public abstract IEnumerable<KeyValuePair<string, string>> SerializeValue(IRequestQueryParamSerializer serializer);

        /// <summary>
        /// Serialize the value into a collection of name -> value pairs using its ToString method
        /// </summary>
        /// <returns>Serialized value</returns>
        public abstract IEnumerable<KeyValuePair<string, string>> SerializeToString();
    }

    /// <summary>
    /// Class containing information about a desired query parameter
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class QueryParameterInfo<T> : QueryParameterInfo
    {
        private readonly string name;
        private readonly T value;

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryParameterInfo{T}"/> class
        /// </summary>
        /// <param name="serializationMethod">Method to use the serialize the query value</param>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        public QueryParameterInfo(QuerySerialializationMethod serializationMethod, string name, T value)
        {
            this.name = name;
            this.value = value;
            this.SerializationMethod = serializationMethod;
        }

        /// <summary>
        /// Serialize the (typed) value into a collection of name -> value pairs using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <returns>Serialized value</returns>
        public override IEnumerable<KeyValuePair<string, string>> SerializeValue(IRequestQueryParamSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            return serializer.SerializeQueryParam<T>(this.name, this.value);
        }

        /// <summary>
        /// Serialize the value into a collection of name -> value pairs using its ToString method
        /// </summary>
        /// <returns>Serialized value</returns>
        public override IEnumerable<KeyValuePair<string, string>> SerializeToString()
        {
            if (this.value == null)
                return Enumerable.Empty<KeyValuePair<string, string>>();

            return new[] { new KeyValuePair<string, string>(this.name, this.value.ToString()) };
        }
    }

    /// <summary>
    /// Class containing information about collection of a desired query parameters
    /// </summary>
    /// <typeparam name="T">Element type of the value</typeparam>
    public class QueryCollectionParameterInfo<T> : QueryParameterInfo
    {
        private readonly string name;
        private readonly IEnumerable<T> values;

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryCollectionParameterInfo{T}"/> class
        /// </summary>
        /// <param name="serializationMethod">Method to use the serialize the query values</param>
        /// <param name="name">Name of the name/values pair</param>
        /// <param name="values">Values of the name/values pair</param>
        public QueryCollectionParameterInfo(QuerySerialializationMethod serializationMethod, string name, IEnumerable<T> values)
        {
            this.name = name;
            this.values = values;
            this.SerializationMethod = serializationMethod;
        }

        /// <summary>
        /// Serialize the (typed) value into a collection of name -> value pairs using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <returns>Serialized value</returns>
        public override IEnumerable<KeyValuePair<string, string>> SerializeValue(IRequestQueryParamSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            return serializer.SerializeQueryCollectionParam<T>(this.name, this.values);
        }

        /// <summary>
        /// Serialize the value into a collection of name -> value pairs using its ToString method
        /// </summary>
        /// <returns>Serialized value</returns>
        public override IEnumerable<KeyValuePair<string, string>> SerializeToString()
        {
            if (this.values == null)
                yield break;

            foreach (var value in this.values)
            {
                if (value != null)
                    yield return new KeyValuePair<string, string>(this.name, value.ToString());
            }
        }
    }
}
