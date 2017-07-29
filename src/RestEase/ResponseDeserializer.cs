using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace RestEase
{
#pragma warning disable CS0618 // Type or member is obsolete
    /// <summary>
    /// Helper capable of deserializing a response, to return to the caller
    /// </summary>
    public abstract class ResponseDeserializer : IResponseDeserializer
    {
        /// <summary>
        /// Read the response string from the response, deserialize, and return a deserialized object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize into</typeparam>
        /// <param name="content">String content read from the response</param>
        /// <param name="response">HttpResponseMessage. Consider calling response.Content.ReadAsStringAsync() to retrieve a string</param>
        /// <returns>Deserialized response</returns>
        [Obsolete("Override Deserialize<T>(string content, HttpResponseMessage response, ResponseDeserializerInfo info) instead")]
        public virtual T Deserialize<T>(string content, HttpResponseMessage response)
        {
            throw new NotImplementedException("You must override and implement Deserialize<T>(string content, HttpResponseMessage response, ResponseDeserializerInfo info)");
        }

        /// <summary>
        /// Read the response string from the response, deserialize, and return a deserialized object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize into</typeparam>
        /// <param name="content">String content read from the response</param>
        /// <param name="response">HttpResponseMessage. Consider calling response.Content.ReadAsStringAsync() to retrieve a string</param>
        /// <param name="info">Extra information about the response</param>
        /// <returns>Deserialized response</returns>
        public virtual T Deserialize<T>(string content, HttpResponseMessage response, ResponseDeserializerInfo info)
        {
            // By default, call the legacy Deserialize<T>(string content, HttpResponseMessage response) method
            return this.Deserialize<T>(content, response);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
