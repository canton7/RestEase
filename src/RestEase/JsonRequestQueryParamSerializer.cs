using System.Collections.Generic;
using Newtonsoft.Json;

namespace RestEase
{
    /// <summary>
    /// Default IRequestParamSerializer, using Json.NET
    /// </summary>
    public class JsonRequestQueryParamSerializer : RequestQueryParamSerializer
    {
        /// <summary>
        /// Gets or sets the serializer settings to pass to JsonConvert.SerializeObject
        /// </summary>
        public JsonSerializerSettings? JsonSerializerSettings { get; set; }
        /// <summary>
        /// Serialize a query parameter whose value is scalar (not a collection), into a collection of name -> value pairs
        /// </summary>
        /// <remarks>
        /// Most of the time, you will only return a single KeyValuePair from this method. However, you are given the flexibility,
        /// to return multiple KeyValuePairs if you wish. Duplicate keys are allowed: they will be serialized as separate query parameters.
        /// </remarks>
        /// <typeparam name="T">Type of the value to serialize</typeparam>
        /// <param name="name">Name of the query parameter</param>
        /// <param name="value">Value of the query parameter</param>
        /// <param name="info">Extra information which may be useful</param>
        /// <returns>A colletion of name -> value pairs to use as query parameters</returns>
        public override IEnumerable<KeyValuePair<string, string?>> SerializeQueryParam<T>(string name, T value, RequestQueryParamSerializerInfo info)
        {
            if (value == null)
                yield break;

            yield return new KeyValuePair<string, string?>(name, JsonConvert.SerializeObject(value, this.JsonSerializerSettings));
        }

        /// <summary>
        /// Serialize a query parameter whose value is a collection, into a collection of name -> value pairs
        /// </summary>
        /// <remarks>
        /// Most of the time, you will return a single KeyValuePair for each value in the collection, and all will have
        /// the same key. However this is not required: you can return whatever you want.
        /// </remarks>
        /// <typeparam name="T">Type of the value to serialize</typeparam>
        /// <param name="name">Name of the query parameter</param>
        /// <param name="values">Values of the query parmaeter</param>
        /// <param name="info">Extra information which may be useful</param>
        /// <returns>A colletion of name -> value pairs to use as query parameters</returns>
        public override IEnumerable<KeyValuePair<string, string?>> SerializeQueryCollectionParam<T>(string name, IEnumerable<T> values, RequestQueryParamSerializerInfo info)
        {
            if (values == null)
                yield break;

            foreach (var value in values)
            {
                if (value != null)
                    yield return new KeyValuePair<string, string?>(name, JsonConvert.SerializeObject(value, this.JsonSerializerSettings));
            }
        }
    }
}
