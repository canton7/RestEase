using Newtonsoft.Json;
using System.Net.Http;

namespace RestEase
{
    /// <summary>
    /// Default IRequestBodySerializer, using Json.NET
    /// </summary>
    public class JsonRequestBodySerializer : RequestBodySerializer
    {
        /// <summary>
        /// Gets or sets the serializer settings to pass to JsonConvert.SerializeObject
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <inheritdoc/>
        public override HttpContent SerializeBody<T>(T body, RequestBodySerializerInfo info)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body, this.JsonSerializerSettings));
            content.Headers.ContentType.MediaType = "application/json";
            return content;
        }
    }
}
