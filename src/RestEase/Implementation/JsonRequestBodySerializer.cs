using Newtonsoft.Json;

namespace RestEase.Implementation
{
    /// <summary>
    /// Default IRequestBodySerializer, using Json.NET
    /// </summary>
    public class JsonRequestBodySerializer : IRequestBodySerializer
    {
        /// <summary>
        /// Gets or sets the serializer settings to pass to JsonConvert.SerializeObject
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <typeparam name="T">Type of the value to serialize</typeparam>
        /// <returns>String suitable for attaching as the requests's Content</returns>
        public string SerializeBody<T>(T body)
        {
            return JsonConvert.SerializeObject(body, this.JsonSerializerSettings);
        }
    }
}
