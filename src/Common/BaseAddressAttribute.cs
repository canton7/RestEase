using System;

namespace RestEase
{
    /// <summary>
    /// Attribute applied to the interface, giving a base address which is used if <see cref="System.Net.Http.HttpClient.BaseAddress"/> is <c>null</c>
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class BaseAddressAttribute : Attribute
    {
        /// <summary>
        /// Gets the base address set in this attribute
        /// </summary>
        public string BaseAddress { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAddressAttribute"/> class with the given base address
        /// </summary>
        /// <param name="baseAddress">Base path to use</param>
        public BaseAddressAttribute(string baseAddress)
        {
            this.BaseAddress = baseAddress;
        }
    }
}
