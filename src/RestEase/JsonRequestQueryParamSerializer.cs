using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    public class JsonRequestQueryParamSerializer : IRequestQueryParamSerializer
    {
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public IEnumerable<KeyValuePair<string, string>> SerializeQueryParam<T>(string name, T value)
        {
            yield return new KeyValuePair<string, string>(name, JsonConvert.SerializeObject(value, this.JsonSerializerSettings));
        }

        public IEnumerable<KeyValuePair<string, string>> SerializeQueryCollectionParam<T>(string name, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                yield return new KeyValuePair<string, string>(name, JsonConvert.SerializeObject(value, this.JsonSerializerSettings));
            }
        }
    }
}
