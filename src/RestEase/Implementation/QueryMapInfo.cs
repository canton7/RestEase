using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    public class QueryMapInfo
    {
        public QuerySerialializationMethod SerializationMethod { get; private set; }

        /// <summary>
        /// Gets or sets the query map value, if specified. Must be an IDictionary or IDictionary{TKey, TValue}
        /// </summary>
        public object Value { get; private set; }

        public QueryMapInfo(QuerySerialializationMethod serializationMethod, object value)
        {
            this.SerializationMethod = serializationMethod;
            this.Value = value;
        }
    }
}
