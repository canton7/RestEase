using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class PathParamAttribute : Attribute
    {
        public string Name { get; set; }

        public PathParamAttribute()
        { }

        public PathParamAttribute(string name)
        {
            this.Name = name;
        }
    }
}
