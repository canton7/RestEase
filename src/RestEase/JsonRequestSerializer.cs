using Newtonsoft.Json;
using System.Net.Http;
using System;

namespace RestEase
{
    /// <summary>
    /// Default IRequestSerializer, using Json.NET
    /// </summary>
    public class JsonRequestSerializer : IRequestSerializer
    {
        /// <summary>
        /// Gets or sets the serializer settings to pass to JsonConvert.SerializeObject
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public string SerializeQueryParameter<T>(T queryParameter)
        {
            return JsonConvert.SerializeObject(queryParameter, this.JsonSerializerSettings);
        }

        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <typeparam name="T">Type of the value to serialize</typeparam>
        /// <returns>HttpContent to assign to the request</returns>
        public HttpContent SerializeBody<T>(T body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body, this.JsonSerializerSettings));
            content.Headers.ContentType.MediaType = "application/json";
            return content;
        }
    }
}
