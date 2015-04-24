using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase
{
    public class JsonResponseDeserializer : IResponseDeserializer
    {
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public async Task<T> ReadAndDeserialize<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            // We fetched using HttpCompletionOption.ResponseContentRead, so this has already been buffered
            // HttpClient has many smarts for working out the correct content encoding (headers, BOMs, etc), and
            // it's much better to trust it (using ReadAsStringAsync) than to try and replicate it (using ReadAsStreamAsync).
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content, this.JsonSerializerSettings);
        }
    }
}
