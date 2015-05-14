using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as being a query param
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class QueryParamAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the query param. Will use the parameter name if null
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryParamAttribute"/> class
        /// </summary>
        public QueryParamAttribute()
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryParamAttribute"/> class, with the given name
        /// </summary>
        /// <param name="name">Name of the query parameter</param>
        public QueryParamAttribute(string name)
        {
            this.Name = name;
        }
    }
}
