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
            // TODO: Figure out how best to handle nullables here. I don't think we can change the signature to return T?
            // without breaking backwards compat... In the meantime, this worked before json.net changed their nullable
            // annotations, so ignore the issue for now
            return JsonConvert.DeserializeObject<T>(content!, this.JsonSerializerSettings)!;
        }
    }
}
