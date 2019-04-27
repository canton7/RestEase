using System;
using System.Collections.Generic;

namespace RestEase
{
    /// <summary>
    /// Helper which knows how to serialize path parameters
    /// </summary>
    public abstract class RequestPathParamSerializer
    {
        /// <summary>
        /// Serialize a path parameter whose value is scalar (not a collection), into a name -> value pair
        /// </summary>
        /// <param name="name">Name of the path parameter</param>
        /// <param name="value">Value of the path parameter</param>
        /// <param name="info">Extra info which may be useful to the serializer</param>
        /// <returns>A name -> value pair to use as path parameter</returns>
        public abstract KeyValuePair<string, string> SerializePathParam(string name,object value, RequestPathParamSerializerInfo info);
    }
}
