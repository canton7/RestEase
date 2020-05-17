using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RestEase.Implementation;
using RestEase.Implementation.Emission;

namespace RestEase.SourceGenerator.Implementation
{
    public class RoslynImplementationFactory
    {
        private readonly Emitter emitter = new Emitter();

        public (SourceText? source, List<Diagnostic> diagnostics) CreateImplementation(INamedTypeSymbol namedTypeSymbol)
        {
            var analyzer = new RoslynTypeAnalyzer(namedTypeSymbol);
            var typeModel = analyzer.Analyze();
            var diagnosticReporter = new DiagnosticReporter(typeModel);
            var generator = new ImplementationGenerator(typeModel, this.emitter, diagnosticReporter);
            var emittedType = generator.Generate();
            return (diagnosticReporter.Diagnostics.Count > 0 ? null : emittedType.SourceText, diagnosticReporter.Diagnostics);
        }
    }
}
