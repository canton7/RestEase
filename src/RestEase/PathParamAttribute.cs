using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as able to substitute a placeholder in this method's path
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class PathParamAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the optional name of the placeholder. Will use the parameter name if null
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="PathParamAttribute"/> class
        /// </summary>
        public PathParamAttribute()
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="PathParamAttribute"/> class, with the given name
        /// </summary>
        /// <param name="name">Placeholder in the path to replace</param>
        public PathParamAttribute(string name)
        {
            this.Name = name;
        }
    }
}
