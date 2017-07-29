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
#pragma warning restore CS0618 // Type or member is obsolete
    {
        /// <summary>
        /// Read the response string from the response, deserialize, and return a deserialized object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize into</typeparam>
        /// <param name="content">String content read from the response</param>
        /// <param name="response">HttpResponseMessage. Consider calling response.Content.ReadAsStringAsync() to retrieve a string</param>
        /// <returns>Deserialized response</returns>
        public abstract T Deserialize<T>(string content, HttpResponseMessage response);
    }
}
