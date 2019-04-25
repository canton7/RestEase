namespace RestEase
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// Default IRequestPathParamSerializer, using Json.NET
    /// </summary>
    public class JsonRequestPathParamSerializer : RequestPathParamSerializer
    {
        /// <summary>
        /// Gets or sets the serializer settings to pass to JsonConvert.SerializeObject
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// Serialize a path parameter whose value is scalar (not a collection), into a name -> value pair
        /// </summary>
        /// <param name="name">Name of the path parameter</param>
        /// <param name="value">Value of the path parameter</param>
        /// <param name="info">Extra info which may be useful to the serializer</param>
        /// <returns>A name -> value pair to use as path parameter</returns>
        public override KeyValuePair<string, string> SerializePathParam(string name, object value, RequestPathParamSerializerInfo info)
        {
            var serializedValue = JsonConvert.SerializeObject(value, this.JsonSerializerSettings);
            return new KeyValuePair<string, string>(name, serializedValue);
        }
    }
}
