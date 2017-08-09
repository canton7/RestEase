using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Default implementation of IResponseDeserializer, using Json.NET
    /// </summary>
    public class JsonResponseDeserializer : DefaultResponseDeserializer
    {
        /// <summary>
        /// Gets or sets the serializer settings to pass to JsonConvert.DeserializeObject{T}
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <inheritdoc/>
        public override async Task<T> Deserialize<T>(HttpResponseMessage response, ResponseDeserializerInfo info)
        {
            var result = await base.Deserialize<T>(response, info);
            if (result != null)
                return result;

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content, this.JsonSerializerSettings);
        }
    }
}
