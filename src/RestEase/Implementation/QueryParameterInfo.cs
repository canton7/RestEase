using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    /// <summary>
    /// Class containing information about a desired query parameter
    /// </summary>
    public abstract class QueryParameterInfo
    {
        /// <summary>
        /// Gets or sets the method to use to serialize the query parameter
        /// </summary>
        public QuerySerialializationMethod SerializationMethod { get; protected set; }

        /// <summary>
        /// Gets the name of the query parameter
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the value of the query parameter, as an object
        /// </summary>
        public object ObjectValue { get; protected set; }

        /// <summary>
        /// Serialize the (typed) value using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <returns>Serialized value</returns>
        public abstract string SerializeValue(IRequestSerializer serializer);
    }

    /// <summary>
    /// Class containing information about a desired query parameter
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class QueryParameterInfo<T> : QueryParameterInfo
    {
        /// <summary>
        /// Gets the value to serialize
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryParameterInfo{T}"/> class
        /// </summary>
        /// <param name="serializationMethod">Method to use the serialize the query value</param>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        public QueryParameterInfo(QuerySerialializationMethod serializationMethod, string name, T value)
        {
            this.Name = name;
            this.SerializationMethod = serializationMethod;
            this.ObjectValue = value;
            this.Value = value;
        }

        /// <summary>
        /// Serialize the (typed) value using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <returns>Serialized value</returns>
        public override string SerializeValue(IRequestSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            if (this.Value == null)
                return null;

            return serializer.SerializeQueryParameter<T>(this.Value);
        }
    }
}
