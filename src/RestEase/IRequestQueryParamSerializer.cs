using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    public interface IRequestQueryParamSerializer
    {
        IEnumerable<KeyValuePair<string, string>> SerializeQueryParam<T>(string name, T value);

        IEnumerable<KeyValuePair<string, string>> SerializeQueryCollectionParam<T>(string name, IEnumerable<T> values);
    }
}
