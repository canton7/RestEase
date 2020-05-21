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
        public (SourceText? source, List<Diagnostic> diagnostics) CreateImplementation(Compilation compilation, INamedTypeSymbol namedTypeSymbol)
        {
            var diagnosticReporter = new DiagnosticReporter();
            var wellKnownSymbols = new WellKnownSymbols(compilation, diagnosticReporter);
            var emitter = new Emitter(wellKnownSymbols);
            var analyzer = new RoslynTypeAnalyzer(namedTypeSymbol, wellKnownSymbols);
            var typeModel = analyzer.Analyze();
            var generator = new ImplementationGenerator(typeModel, emitter, diagnosticReporter);
            var emittedType = generator.Generate();
            return (diagnosticReporter.Diagnostics.Count > 0 ? null : emittedType.SourceText, diagnosticReporter.Diagnostics);
        }
    }
}
