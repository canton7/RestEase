using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class QueryParamAttribute : Attribute
    {
        public string Name { get; set; }

        public QueryParamAttribute()
        { }

        public QueryParamAttribute(string name)
        {
            this.Name = name;
        }
    }
}
