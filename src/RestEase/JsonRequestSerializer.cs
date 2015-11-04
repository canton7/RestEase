using Newtonsoft.Json;
using System.Net.Http;

namespace RestEase
{
    /// <summary>
    /// Default IRequestSerializer, using Json.NET
    /// </summary>
    public class JsonRequestSerializer : IRequestSerializer
    {
        /// <summary>
        /// Gets or sets the serializer settings to pass to JsonConvert.SerializeObject
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// Serialize the given query parameter value
        /// </summary>
        /// <typeparam name="T">Type of the query parameter value</typeparam>
        /// <param name="queryParameterValue">Query parameter value to serialize</param>
        /// <returns>Serialized value</returns>
        public string SerializeQueryParameter<T>(T queryParameterValue)
        {
            return JsonConvert.SerializeObject(queryParameterValue, this.JsonSerializerSettings);
        }

        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <typeparam name="T">Type of the value to serialize</typeparam>
        /// <returns>HttpContent to assign to the request</returns>
        public HttpContent SerializeBody<T>(T body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body, this.JsonSerializerSettings));
            content.Headers.ContentType.MediaType = "application/json";
            return content;
        }
    }
}
