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
    public abstract class ResponseDeserializerBase : IResponseDeserializer
    {
        public Encoding ResponseEncoding { get; set; }

        public ResponseDeserializerBase()
        {
            this.ResponseEncoding = Encoding.UTF8;
        }

        protected virtual async Task<string> ReadStringFromResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            int responseLength = (int)(response.Content.Headers.ContentLength ?? 0);
            string responseString;
            using (var ms = new MemoryStream(responseLength))
            {
                var fromStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                await fromStream.CopyToAsync(ms, 4096, cancellationToken).ConfigureAwait(false);

                responseString = this.ResponseEncoding.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }

            return responseString;
        }

        public abstract T DeserializeString<T>(string content);

        public virtual async Task<T> ReadAndDeserialize<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var responseString = await this.ReadStringFromResponseAsync(response, cancellationToken).ConfigureAwait(false);
            T deserializedResponse = this.DeserializeString<T>(responseString);
            return deserializedResponse;
        }
    }
}
