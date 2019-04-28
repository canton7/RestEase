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
        /// Serialize a path parameter whose value is scalar (not a collection), into a string value
        /// </summary>
        /// <typeparam name="T">Type of the value to serialize</typeparam>
        /// <param name="value">Value of the path parameter</param>
        /// <param name="info">Extra info which may be useful to the serializer</param>
        /// <returns>A string value to use as path parameter</returns>
        public abstract string SerializePathParam<T>(T value, RequestPathParamSerializerInfo info);
    }
}
