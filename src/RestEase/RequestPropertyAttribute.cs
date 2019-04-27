using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as HTTP request message property
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class RequestPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the optional key of the parameter. Will use the parameter name if null
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="PathAttribute"/> class
        /// </summary>
        public RequestPropertyAttribute()
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="PathAttribute"/> class, with the given key
        /// </summary>
        /// <param name="key">key</param>
        public RequestPropertyAttribute(string key)
        {
            this.Key = key;
        }
    }
}