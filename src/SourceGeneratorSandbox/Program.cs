using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;

namespace SourceGeneratorSandbox
{
    class Program
    {
        static void Main()
        {
            var impl = RestClient.For<ISomeApi>("https://api.example.com");
        }

        public interface ISomeApi
        {
            [Get("foo/{bar}")]
            Task FooAsync([Path] string bar, string yay);
        }
    }
}

