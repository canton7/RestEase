using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    /// <summary>
    /// Default IRequestBodySerializer, using Json.NET
    /// </summary>
    public class JsonRequestBodySerializer : IRequestBodySerializer
    {
        /// <summary>
        /// Serializer settings to pass to JsonConvert.SerializeObject
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <returns>String suitable for attaching as the requests's Content</returns>
        public string SerializeBody(object body)
        {
            return JsonConvert.SerializeObject(body, this.JsonSerializerSettings);
        }
    }
}
