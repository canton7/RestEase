using Microsoft.CodeAnalysis;
using RestEase.SourceGenerator.Implementation;

namespace RestEase.SourceGenerator
{
    [Generator]
    public class RestEaseSourceGenerator : ISourceGenerator
    {
        public void Execute(SourceGeneratorContext context)
        {
            new Processor(context).Process();
        }

        public void Initialize(InitializationContext context)
        {
        }
    }
}
