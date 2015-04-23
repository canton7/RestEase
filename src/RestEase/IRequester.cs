using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    public interface IRequester
    {
        Task RequestVoidAsync(RequestInfo requestInfo);
        Task<T> RequestAsync<T>(RequestInfo requestInfo);
        Task<Response<T>> RequestWithResponseAsync<T>(RequestInfo requestInfo);
    }
}
