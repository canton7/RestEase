using System;
using System.Collections.Generic;
using System.Text;

namespace RestEase.Implementation
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RestEaseInterfaceImplementationAttribute : Attribute
    {
        public Type InterfaceType { get; }
        public Type ImplementationType { get; }

        public RestEaseInterfaceImplementationAttribute(Type interfaceType, Type implementationType)
        {
            this.InterfaceType = interfaceType;
            this.ImplementationType = implementationType;
        }
    }
}
