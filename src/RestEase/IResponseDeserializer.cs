using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Helper capable of deserializing a response, to return to the caller
    /// </summary>
    public interface IResponseDeserializer
    {
        /// <summary>
        /// Read the response string from the response, deserialize, and return a deserialized object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize into</typeparam>
        /// <param name="response">HttpResponseMessage. Consider calling response.Content.ReadAsStringAsync() to retrieve a string</param>
        /// <param name="cancellationToken">CancellationToken for this request</param>
        /// <returns>Deserialized response</returns>
        Task<T> ReadAndDeserializeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken);
    }
}
