using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    public enum BodySerializationMethod
    {
        Serialized,
        UrlEncoded,
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class BodyAttribute : Attribute
    {
        public BodySerializationMethod SerializationMethod { get; private set; }

        public BodyAttribute()
        {
            this.SerializationMethod = BodySerializationMethod.Serialized;
        }

        public BodyAttribute(BodySerializationMethod serializationMethod)
        {
            this.SerializationMethod = serializationMethod;
        }
    }
}
