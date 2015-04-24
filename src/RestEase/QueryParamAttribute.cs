using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as being a query param
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class QueryParamAttribute : Attribute
    {
        /// <summary>
        /// Name of the query param. Will use the parameter name if null
        /// </summary>
        public string Name { get; set; }

        public QueryParamAttribute()
        { }

        public QueryParamAttribute(string name)
        {
            this.Name = name;
        }
    }
}
