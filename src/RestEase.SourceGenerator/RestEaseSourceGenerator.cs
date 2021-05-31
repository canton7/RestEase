using Microsoft.CodeAnalysis;
using RestEase.SourceGenerator.Implementation;

namespace RestEase.SourceGenerator
{
    [Generator]
    public class RestEaseSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            new Processor(context).Process();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }
}
