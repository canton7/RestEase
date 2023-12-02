using System;

namespace RestEase
{
    /// <summary>
    /// Attribute applied to the interface, giving a path which is prepended to all paths 
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class BasePathAttribute : Attribute
    {
        /// <summary>
        /// Gets the base path set in this attribute
        /// </summary>
        public string BasePath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePathAttribute"/> class with the given base path
        /// </summary>
        /// <param name="basePath">Base path to use</param>
        public BasePathAttribute(string basePath)
        {
            this.BasePath = basePath;
        }
    }
}
