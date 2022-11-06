using System;
using System.Net.Http;

namespace RestEase
{
#pragma warning disable CS0618 // Type or member is obsolete
    /// <summary>
    /// Helper capable of deserializing a response, to return to the caller
    /// </summary>
    public abstract class ResponseDeserializer : IResponseDeserializer
    {
        /// <summary>
        /// Gets or sets a value indicating whether this deserializer can deserialize strings
        /// </summary>
        /// <remarks>
        /// If <c>true</c>, interface methods which return <c>Task{string}</c> (or <c>Task{Response{string}}</c>
        /// result in the response being passed through this deserializer. If <c>false</c>, such methods result in
        /// the raw response being returned, and not passed through this deserializer.
        /// 
        /// The default value is <c>false</c>.
        /// </remarks>
        public bool HandlesStrings { get; set; } = false;

        [Obsolete("Override Deserialize<T>(string content, HttpResponseMessage response, ResponseDeserializerInfo info) instead", error: true)]
        T IResponseDeserializer.Deserialize<T>(string? content, HttpResponseMessage response)
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
        public virtual T Deserialize<T>(string? content, HttpResponseMessage response, ResponseDeserializerInfo info)
        {
            throw new NotImplementedException($"You must override and implement T Deserialize<T>(string content, HttpResponseMessage response, ResponseDeserializerInfo info) in {this.GetType().Name}");
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
