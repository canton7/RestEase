using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Helper which knows how to serialize a request body
    /// </summary>
    public interface IRequestBodySerializer
    {
        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <returns>String suitable for attaching as the requests's Content</returns>
        string SerializeBody(object body);
    }
}
