using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase
{
    public class JsonResponseDeserializer : ResponseDeserializerBase
    {
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public override T DeserializeString<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content, this.JsonSerializerSettings);
        }
    }
}
