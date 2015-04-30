using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    internal struct IndexedParameter
    {
        public readonly int Index;
        public readonly ParameterInfo Parameter;

        public IndexedParameter(int index, ParameterInfo parameter)
        {
            this.Index = index;
            this.Parameter = parameter;
        }
    }

    internal struct IndexedParameter<T> where T : Attribute
    {
        public readonly int Index;
        public readonly ParameterInfo Parameter;
        public readonly T Attribute;

        public IndexedParameter(int index, ParameterInfo parameter, T attribute)
        {
            this.Index = index;
            this.Parameter = parameter;
            this.Attribute = attribute;
        }
    }
}
