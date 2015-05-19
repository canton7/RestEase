using Newtonsoft.Json;
using System.Net.Http;

namespace RestEase
{
    /// <summary>
    /// Default IRequestBodySerializer, using Json.NET
    /// </summary>
    public class JsonRequestBodySerializer : IRequestBodySerializer
    {
        /// <summary>
        /// Gets or sets the serializer settings to pass to JsonConvert.SerializeObject
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <typeparam name="T">Type of the value to serialize</typeparam>
        /// <returns>HttpContent to assign to the request</returns>
        public HttpContent SerializeBody<T>(T body)
        {
            return new StringContent(JsonConvert.SerializeObject(body, this.JsonSerializerSettings));
        }
    }
}
