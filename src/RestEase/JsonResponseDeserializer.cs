using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

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
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <inheritdoc/>
        public override async Task<T> DeserializeAsync<T>(HttpResponseMessage response, ResponseDeserializerInfo info)
        {
            if (typeof(T) == typeof(Stream))
                return (T)(object)(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));

            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (typeof(T) == typeof(string))
                return (T)(object)content;

            return JsonConvert.DeserializeObject<T>(content, this.JsonSerializerSettings);
        }
    }
}
