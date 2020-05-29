using System;
using System.ComponentModel;

namespace RestEase.Implementation
{
    /// <summary>
    /// Internal type. Do not use.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RestEaseInterfaceImplementationAttribute : Attribute
    {
        /// <summary>
        /// Internal type. Do not use.
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// Internal type. Do not use.
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// Internal type. Do not use.
        /// </summary>
        public RestEaseInterfaceImplementationAttribute(Type interfaceType, Type implementationType)
        {
            this.InterfaceType = interfaceType;
            this.ImplementationType = implementationType;
        }
    }
}
