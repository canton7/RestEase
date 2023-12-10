using System;

namespace RestEase
{
    /// <summary>
    /// Controls whether the given method, or all methods within the given interface, will throw an exception if the response status code does not indicate success
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class AllowAnyStatusCodeAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether to suppress the exception normally thrown on responses that do not indicate success
        /// </summary>
        public bool AllowAnyStatusCode { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="AllowAnyStatusCodeAttribute"/> class, which does allow any status code
        /// </summary>
        public AllowAnyStatusCodeAttribute()
            : this(true)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="AllowAnyStatusCodeAttribute"/>.
        /// </summary>
        /// <param name="allowAnyStatusCode">True to allow any response status code; False to throw an exception on response status codes that do not indicate success</param>
        public AllowAnyStatusCodeAttribute(bool allowAnyStatusCode)
        {
            this.AllowAnyStatusCode = allowAnyStatusCode;
        }
    }
}
