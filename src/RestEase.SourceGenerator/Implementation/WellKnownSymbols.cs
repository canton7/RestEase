using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RestEase.Implementation.Emission;

namespace RestEase.SourceGenerator.Implementation
{
    internal class WellKnownSymbols
    {
        private readonly IAssemblySymbol restEaseAssembly;
        private readonly DiagnosticReporter diagnosticReporter;

        private INamedTypeSymbol? allowAnyStatusCodeAttribute;
        public INamedTypeSymbol? AllowAnyStatusCodeAttribute => this.allowAnyStatusCodeAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.AllowAnyStatusCodeAttribute));

        private INamedTypeSymbol? basePathAttribute;
        public INamedTypeSymbol? BasePathAttribute => this.basePathAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.BasePathAttribute));

        private INamedTypeSymbol? pathAttribute;
        public INamedTypeSymbol? PathAttribute => this.pathAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.PathAttribute));

        private INamedTypeSymbol? queryAttribute;
        public INamedTypeSymbol? QueryAttribute => this.queryAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.QueryAttribute));

        private INamedTypeSymbol? serializationMethodsAttribute;
        public INamedTypeSymbol? SerializationMethodsAttribute => this.serializationMethodsAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.SerializationMethodsAttribute));

        private INamedTypeSymbol? getAttribute;
        public INamedTypeSymbol? GetAttribute => this.getAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.GetAttribute));

        private INamedTypeSymbol? pathSerializationMethod;
        public INamedTypeSymbol? PathSerializationMethod => this.pathSerializationMethod ??= this.LookupLocal("RestEase." + nameof(RestEase.PathSerializationMethod));

        private INamedTypeSymbol? querySerializationMethod;
        public INamedTypeSymbol? QuerySerializationMethod => this.querySerializationMethod ??= this.LookupLocal("RestEase." + nameof(RestEase.QuerySerializationMethod));

        private INamedTypeSymbol? bodySerializationMethod;
        public INamedTypeSymbol? BodySerializationMethod => this.bodySerializationMethod ??= this.LookupLocal("RestEase." + nameof(RestEase.BodySerializationMethod));

        private INamedTypeSymbol? rawQueryStringAttribute;
        public INamedTypeSymbol? RawQueryStringAttribute => this.rawQueryStringAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.RawQueryStringAttribute));

        private INamedTypeSymbol? bodyAttribute;
        public INamedTypeSymbol? BodyAttribute => this.bodyAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.BodyAttribute));

        private INamedTypeSymbol? headerAttribute;
        public INamedTypeSymbol? HeaderAttribute => this.headerAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.HeaderAttribute));

        private INamedTypeSymbol? httpRequestMessagePropertyAttribute;
        public INamedTypeSymbol? HttpRequestMessagePropertyAttribute => this.httpRequestMessagePropertyAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.HttpRequestMessagePropertyAttribute));

        private INamedTypeSymbol? cancellationToken;
        public INamedTypeSymbol? CancellationToken => this.cancellationToken ??= this.LookupSystem("System.Threading.CancellationToken");

        private INamedTypeSymbol? task;
        public INamedTypeSymbol? Task => this.task ??= this.LookupSystem("System.Threading.Tasks.Task");

        public WellKnownSymbols(Compilation compilation, DiagnosticReporter diagnosticReporter)
        {
            this.restEaseAssembly = GetReferencedAssemblies(compilation.Assembly)
                .Single(x => x.Name == "RestEase");
            this.diagnosticReporter = diagnosticReporter;
        }

        private static IEnumerable<IAssemblySymbol> GetReferencedAssemblies(IAssemblySymbol assembly)
        {
            return assembly.Modules.SelectMany(x => x.ReferencedAssemblySymbols);
        }

        private INamedTypeSymbol? LookupLocal(string metadataName)
        {
            var type = this.restEaseAssembly.GetTypeByMetadataName(metadataName);
            if (type == null)
            {
                this.diagnosticReporter.ReportCouldNotFindRestEaseType(metadataName);
            }
            return type;
        }

        private INamedTypeSymbol? LookupSystem(string metadataName)
        {
            // We search the assemblies that the RestEase assembly references. That way we can be fairly sure
            // that noone's created a time with the same name.
            foreach (var assembly in GetReferencedAssemblies(this.restEaseAssembly))
            {
                // RestEase can reference netstandard, which forwards types
                var type = assembly.GetTypeByMetadataName(metadataName) ?? assembly.ResolveForwardedType(metadataName);
                if (type != null)
                {
                    return type;
                }
            }

            this.diagnosticReporter.ReportCouldNotFindSystemType(metadataName);
            return null;
        }
    }
}
