using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Attribute allowing interface-level, method-level, or parameter-level headers to be defined. See the docs for details
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class HeaderAttribute : Attribute
    {
        /// <summary>
        /// Header value
        /// </summary>
        public string Value { get; set; }

        public HeaderAttribute(string value)
        {
            this.Value = value;
        }
    }
}
