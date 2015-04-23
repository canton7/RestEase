using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class HeaderAttribute : Attribute
    {
        public string Value { get; set; }

        public HeaderAttribute(string value)
        {
            this.Value = value;
        }
    }
}
