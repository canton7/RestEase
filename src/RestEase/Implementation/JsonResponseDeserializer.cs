using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase.Implementation
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
        /// <param name="response">HttpResponseMessage. Consider calling response.Content.ReadAsStringAsync() to retrieve a string</param>
        /// <param name="cancellationToken">CancellationToken for this request</param>
        /// <returns>Deserialized response</returns>
        public async Task<T> ReadAndDeserializeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            // We fetched using HttpCompletionOption.ResponseContentRead, so this has already been buffered
            // HttpClient has many smarts for working out the correct content encoding (headers, BOMs, etc), and
            // it's much better to trust it (using ReadAsStringAsync) than to try and replicate it (using ReadAsStreamAsync).
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content, this.JsonSerializerSettings);
        }
    }
}
