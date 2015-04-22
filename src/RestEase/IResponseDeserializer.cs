using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase
{
    public interface IResponseDeserializer
    {
        Task<T> ReadAndDeserialize<T>(HttpResponseMessage response, CancellationToken cancellationToken);
    }
}
