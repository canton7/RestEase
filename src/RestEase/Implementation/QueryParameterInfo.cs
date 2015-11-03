using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    public abstract class QueryParameterInfo
    {
        public QuerySerialializationMethod SerializationMethod { get; protected set; }

        public string Name { get; protected set; }

        public abstract object ObjectValue { get; }

        public abstract string SerializeValue(IRequestBodySerializer serializer);
    }

    public class QueryParameterInfo<T> : QueryParameterInfo
    {
        public T Value { get; private set; }

        public override object ObjectValue
        {
            get { return this.Value; }
        }

        public QueryParameterInfo(QuerySerialializationMethod serializationMethod, string name, T value)
        {
            this.Name = name;
            this.SerializationMethod = serializationMethod;
            this.Value = value;
        }

        public override string SerializeValue(IRequestBodySerializer serializer)
        {
            throw new NotImplementedException();   
        }
    }
}
