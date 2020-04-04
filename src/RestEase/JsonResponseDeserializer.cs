using Newtonsoft.Json;
using System.Net.Http;

namespace RestEase
{
    /// <summary>
    /// Default implementation of IResponseDeserializer, using Json.NET
    /// </summary>
    public class JsonResponseDeserializer : ResponseDeserializer
    {
        /// <summary>
        /// Gets or sets the serializer settings to pass to JsonConvert.DeserializeObject{T}
        /// </summary>
        public JsonSerializerSettings? JsonSerializerSettings { get; set; }

        /// <inheritdoc/>
        public override T Deserialize<T>(string? content, HttpResponseMessage response, ResponseDeserializerInfo info)
        {
            return JsonConvert.DeserializeObject<T>(content, this.JsonSerializerSettings);
        }
    }
}
