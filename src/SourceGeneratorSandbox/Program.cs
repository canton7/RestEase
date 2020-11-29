using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;

namespace SourceGeneratorSandbox
{
    public class Program
    {
        public static void Main()
        {
            var impl = RestClient.For<ISomeApi>("https://api.example.com");
            impl.FooAsync("test", "test").Wait();
        }

        public interface ISomeApi
        {
            [Get("foo/{bar}")]
            Task FooAsync([Path] string bar, string baz);
        }
    }
}

