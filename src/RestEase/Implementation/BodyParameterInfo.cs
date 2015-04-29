using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    /// <summary>
    /// Class containing information about a desired requwest body
    /// </summary>
    public class BodyParameterInfo
    {
        /// <summary>
        /// Gets the method to use to serialize the body
        /// </summary>
        public BodySerializationMethod SerializationMethod { get; private set; }

        /// <summary>
        /// Gets the body to serialize
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="BodyParameterInfo"/> class
        /// </summary>
        /// <param name="serializationMethod">Method to use the serialize the body</param>
        /// <param name="value">Body to serialize</param>
        public BodyParameterInfo(BodySerializationMethod serializationMethod, object value)
        {
            this.SerializationMethod = serializationMethod;
            this.Value = value;
        }
    }
}
