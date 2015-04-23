using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    public interface IRequestBodySerializer
    {
        string SerializeBody(object body);
    }
}
