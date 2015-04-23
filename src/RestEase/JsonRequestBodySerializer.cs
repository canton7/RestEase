using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    public class JsonRequestBodySerializer : IRequestBodySerializer
    {
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public string SerializeBody(object body)
        {
            return JsonConvert.SerializeObject(body, this.JsonSerializerSettings);
        }
    }
}
