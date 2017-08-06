using System;
using System.Collections.Generic;

namespace RestEase
{
    /// <summary>
    /// Helper which knows how to serialize query parmaeters
    /// </summary>
    [Obsolete("Use RequestQueryParamSerializer instead")]
    public interface IRequestQueryParamSerializer
    {
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
        /// <param name="info">Extra info which may be useful to the serializer</param>
        /// <returns>A colletion of name -> value pairs to use as query parameters</returns>
        IEnumerable<KeyValuePair<string, string>> SerializeQueryParam<T>(string name, T value, RequestQueryParamSerializerInfo info);

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
        /// <param name="info">Extra info which may be useful to the serializer</param>
        /// <returns>A colletion of name -> value pairs to use as query parameters</returns>
        IEnumerable<KeyValuePair<string, string>> SerializeQueryCollectionParam<T>(string name, IEnumerable<T> values, RequestQueryParamSerializerInfo info);
    }
}
