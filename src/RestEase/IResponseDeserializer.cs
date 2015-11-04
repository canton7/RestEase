using System.Net.Http;

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
        /// <param name="content">String content read from the response</param>
        /// <param name="response">HttpResponseMessage. Consider calling response.Content.ReadAsStringAsync() to retrieve a string</param>
        /// <returns>Deserialized response</returns>
        T Deserialize<T>(string content, HttpResponseMessage response);
    }
}
