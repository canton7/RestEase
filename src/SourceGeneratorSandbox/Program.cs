using System;
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

        public interface ISomeApi
        {
            [Get("foo")]
            Task FooAsync(int foo);
    }
}

