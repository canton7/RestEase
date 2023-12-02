using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as HTTP request message property
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class HttpRequestMessagePropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the optional key of the parameter. Will use the parameter name if null
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestMessagePropertyAttribute"/> class
        /// </summary>
        public HttpRequestMessagePropertyAttribute()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestMessagePropertyAttribute"/> class, with the given key
        /// </summary>
        /// <param name="key">key</param>
        public HttpRequestMessagePropertyAttribute(string key)
        {
            this.Key = key;
        }
    }
}