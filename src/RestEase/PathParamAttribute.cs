using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as able to substitute a placeholder in this method's path
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class PathParamAttribute : Attribute
    {
        /// <summary>
        /// Optional name of the placeholder. Will use the parameter name if null
        /// </summary>
        public string Name { get; set; }

        public PathParamAttribute()
        { }

        public PathParamAttribute(string name)
        {
            this.Name = name;
        }
    }
}
