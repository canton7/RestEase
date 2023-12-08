﻿using System;
using System.Collections.Generic;

namespace RestEase.Implementation
{
    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// Class containing information about a desired path parameter
    /// </summary>
    public abstract class PathParameterInfo
    {
        /// <summary>
        /// Gets a value indicating whether this path parameter should be URL-encoded
        /// </summary>
        public bool UrlEncode { get; protected set; }

        /// <summary>
        /// Gets the method to use to serialize the path parameter.
        /// </summary>
        public PathSerializationMethod SerializationMethod { get; protected set; }

        /// <summary>
        /// Serialize the value into a name -> value pair using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public abstract KeyValuePair<string, string?> SerializeValue(RequestPathParamSerializer serializer, IRequestInfo requestInfo, IFormatProvider? formatProvider);

        /// <summary>
        /// Serialize the value into a name -> value pair using its ToString method
        /// </summary>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public abstract KeyValuePair<string, string?> SerializeToString(IFormatProvider? formatProvider);
    }

    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// Class containing information about a desired path parameter
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class PathParameterInfo<T> : PathParameterInfo
    {
        private readonly string name;
        private readonly T value;
        private readonly string? format;

        /// <summary>
        /// Initialises a new instance of the <see cref="PathParameterInfo{T}"/> Structure
        /// </summary>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        /// <param name="format">Format string to use</param>
        /// <param name="urlEncode">Indicates whether this parameter should be url-encoded</param>
        /// <param name="serializationMethod">Method to use to serialize the path value.</param>
        public PathParameterInfo(string name, T value, string? format, bool urlEncode, PathSerializationMethod serializationMethod)
        {
            this.name = name;
            this.value = value;
            this.format = format;
            this.UrlEncode = urlEncode;
            this.SerializationMethod = serializationMethod;
        }

        /// <summary>
        /// Serialize the value into a name -> value pair using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public override KeyValuePair<string, string?> SerializeValue(RequestPathParamSerializer serializer, IRequestInfo requestInfo, IFormatProvider? formatProvider)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (requestInfo == null)
                throw new ArgumentNullException(nameof(requestInfo));

            string? serializedValue = serializer.SerializePathParam(this.value, new RequestPathParamSerializerInfo(requestInfo, this.format, formatProvider));
            return new KeyValuePair<string, string?>(this.name, serializedValue);
        }

        /// <summary>
        /// Serialize the value into a name -> value pair using its ToString method
        /// </summary>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public override KeyValuePair<string, string?> SerializeToString(IFormatProvider? formatProvider)
        {
            return new KeyValuePair<string, string?>(this.name, ToStringHelper.ToString(this.value, this.format, formatProvider));
        }
    }
}
