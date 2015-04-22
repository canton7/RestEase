using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class AliasAsAttribute : Attribute
    {
        public string Name { get; set; }

        public AliasAsAttribute()
        { }

        public AliasAsAttribute(string name)
        {
            this.Name = name;
        }
    }
}
