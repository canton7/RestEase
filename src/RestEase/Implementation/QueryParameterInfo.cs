﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RestEase.Implementation
{
    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// Class containing information about a desired query parameter
    /// </summary>
    public abstract class QueryParameterInfo
    {
        /// <summary>
        /// Gets or sets the method to use to serialize the query parameter
        /// </summary>
        public QuerySerializationMethod SerializationMethod { get; protected set; }

        /// <summary>
        /// Serialize the (typed) value into a collection of name -> value pairs using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public abstract IEnumerable<KeyValuePair<string, string?>> SerializeValue(RequestQueryParamSerializer serializer, IRequestInfo requestInfo, IFormatProvider? formatProvider);

        /// <summary>
        /// Serialize the value into a collection of name -> value pairs using its ToString method
        /// </summary>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public abstract IEnumerable<KeyValuePair<string, string?>> SerializeToString(IFormatProvider? formatProvider);
    }

    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// Class containing information about a desired query parameter
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class QueryParameterInfo<T> : QueryParameterInfo
    {
        private readonly string name;
        private readonly T value;
        private readonly string? format;

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryParameterInfo{T}"/> class
        /// </summary>
        /// <param name="serializationMethod">Method to use the serialize the query value</param>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        /// <param name="format">Format string to use</param>
        public QueryParameterInfo(QuerySerializationMethod serializationMethod, string name, T value, string? format)
        {
            this.SerializationMethod = serializationMethod;
            this.name = name;
            this.value = value;
            this.format = format;
        }

        /// <summary>
        /// Serialize the (typed) value into a collection of name -> value pairs using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public override IEnumerable<KeyValuePair<string, string?>> SerializeValue(RequestQueryParamSerializer serializer, IRequestInfo requestInfo, IFormatProvider? formatProvider)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (requestInfo == null)
                throw new ArgumentNullException(nameof(requestInfo));

            return serializer.SerializeQueryParam(this.name, this.value, new RequestQueryParamSerializerInfo(requestInfo, this.format, formatProvider));
        }

        /// <summary>
        /// Serialize the value into a collection of name -> value pairs using its ToString method
        /// </summary>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public override IEnumerable<KeyValuePair<string, string?>> SerializeToString(IFormatProvider? formatProvider)
        {
            if (this.value == null)
                return Enumerable.Empty<KeyValuePair<string, string?>>();

            return new[] { new KeyValuePair<string, string?>(this.name, ToStringHelper.ToString(this.value, this.format, formatProvider)) };
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
        private readonly string? format;

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryCollectionParameterInfo{T}"/> class
        /// </summary>
        /// <param name="serializationMethod">Method to use the serialize the query values</param>
        /// <param name="name">Name of the name/values pair</param>
        /// <param name="values">Values of the name/values pair</param>
        /// <param name="format">Format string to use</param>
        public QueryCollectionParameterInfo(QuerySerializationMethod serializationMethod, string name, IEnumerable<T> values, string? format)
        {
            this.SerializationMethod = serializationMethod;
            this.name = name;
            this.values = values;
            this.format = format;
        }

        /// <summary>
        /// Serialize the (typed) value into a collection of name -> value pairs using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public override IEnumerable<KeyValuePair<string, string?>> SerializeValue(RequestQueryParamSerializer serializer, IRequestInfo requestInfo, IFormatProvider? formatProvider)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            return serializer.SerializeQueryCollectionParam(this.name, this.values, new RequestQueryParamSerializerInfo(requestInfo, this.format, formatProvider));
        }

        /// <summary>
        /// Serialize the value into a collection of name -> value pairs using its ToString method
        /// </summary>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public override IEnumerable<KeyValuePair<string, string?>> SerializeToString(IFormatProvider? formatProvider)
        {
            if (this.values == null)
                yield break;

            foreach (var value in this.values)
            {
                if (value == null)
                    continue;

                yield return new KeyValuePair<string, string?>(this.name, ToStringHelper.ToString(value, this.format, formatProvider));
            }
        }
    }
}
