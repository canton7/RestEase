using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    public class Response<T>
    {
        public HttpResponseMessage ResponseMessage { get; private set; }
        public T Content { get; private set; }

        public Response(HttpResponseMessage response, T content)
        {
            this.ResponseMessage = response;
            this.Content = content;
        }
    }
}
