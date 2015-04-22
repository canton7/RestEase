using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    public class RestEaseImplementationCreationException : Exception
    {
        public RestEaseImplementationCreationException(string message)
            : base(message)
        { }

        public RestEaseImplementationCreationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
