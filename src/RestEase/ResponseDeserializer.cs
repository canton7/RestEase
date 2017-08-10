using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
#pragma warning disable CS0618 // Type or member is obsolete
    /// <summary>
    /// Helper capable of deserializing a response, to return to the caller
    /// </summary>
    public abstract class ResponseDeserializer : IResponseDeserializer
    {
        [Obsolete("Call DeserializeAsync<T>(HttpResponseMessage response, ResponseDeserializerInfo info) instead", error: true)]
        T IResponseDeserializer.Deserialize<T>(string content, HttpResponseMessage response)
        {
            // This exists only so that we can assign instances of ResponseDeserializer to the IResponseDeserializer in RestClient
            throw new InvalidOperationException("This should never be called");
        }

        /// <summary>
        /// Read the response string from the response, deserialize, and return a deserialized object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize into</typeparam>
        /// <param name="content">String content read from the response</param>
        /// <param name="response">HttpResponseMessage. Consider calling response.Content.ReadAsStringAsync() to retrieve a string</param>
        /// <param name="info">Extra information about the response</param>
        /// <returns>Deserialized response</returns>
        [Obsolete("Override DeserializeAsync<T>(HttpResponseMessage response, ResponseDeserializerInfo info)")]
        public virtual T Deserialize<T>(string content, HttpResponseMessage response, ResponseDeserializerInfo info)
        {
            throw new NotImplementedException("You must override and implement T Deserialize<T>(string content, HttpResponseMessage response, ResponseDeserializerInfo info)");
        }

        public virtual async Task<T> DeserializeAsync<T>(HttpResponseMessage response, ResponseDeserializerInfo info)
        {
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return this.Deserialize<T>(content, response, info);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
