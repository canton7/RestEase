using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Default implementation of IResponseDeserializer, dispatching
    /// </summary>
    public class DefaultResponseDeserializer : ResponseDeserializer
    {
        /// <inheritdoc/>
        public override async Task<T> Deserialize<T>(HttpResponseMessage response, ResponseDeserializerInfo info)
        {
            var typeofT = typeof(T);

            if (typeofT == typeof(string))
                return (T)(object)await response.Content.ReadAsStringAsync();

            else if (typeofT == typeof(Stream))
                return (T)(object)await response.Content.ReadAsStreamAsync();

            else if (typeofT == typeof(HttpResponseMessage))
                return (T)(object)response;

            return default(T);
        }
    }
}
