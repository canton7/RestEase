using Newtonsoft.Json;
using System.Net.Http;

namespace RestEase
{
    /// <summary>
    /// Default implementation of IResponseDeserializer, using Json.NET
    /// </summary>
    public class JsonResponseDeserializer : IResponseDeserializer
    {
        /// <summary>
        /// Gets or sets the serializer settings to pass to JsonConvert.DeserializeObject{T}
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// Read the response string from the response, deserialize, and return a deserialized object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize into</typeparam>
        /// <param name="content">String content read from the response</param>
        /// <param name="response">HttpResponseMessage. Consider calling response.Content.ReadAsStringAsync() to retrieve a string</param>
        /// <returns>Deserialized response</returns>
        public T Deserialize<T>(string content, HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<T>(content, this.JsonSerializerSettings);
        }
    }
}
