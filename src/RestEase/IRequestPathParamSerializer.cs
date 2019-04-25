namespace RestEase
{
    using System.Collections.Generic;

    /// <summary>
    /// Helper which knows how to serialize path parameters
    /// </summary>
    public interface IRequestPathParamSerializer
    {
        /// <summary>
        /// Serialize a path parameter whose value is scalar (not a collection), into a of name -> value pair
        /// </summary>
        /// <param name="name">Name of the path parameter</param>
        /// <param name="value">Value of the path parameter</param>
        /// <param name="info">Extra info which may be useful to the serializer</param>
        /// <returns>A name -> value pair to use as path parameter</returns>
        KeyValuePair<string, string> SerializePathParam(string name, object value, RequestPathParamSerializerInfo info);
    }
}
