using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;

namespace SourceGeneratorSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        public interface IHasDuplicatePathParams
        {
            [Get("foo/{bar}")]
            Task FooAsync([Path] string bar, [Path("bar")] string yay);
        }
    }
}

