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

        private INamedTypeSymbol? queryMapAttribute;
        public INamedTypeSymbol? QueryMapAttribute => this.queryMapAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.QueryMapAttribute));

        private INamedTypeSymbol? responseT;
        public INamedTypeSymbol? ResponseT => this.responseT ??= this.LookupLocal("RestEase.Response`1");

        private INamedTypeSymbol? requestAttribute;
        public INamedTypeSymbol? RequestAttribute => this.requestAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.RequestAttribute));

        private INamedTypeSymbol? deleteAttribute;
        public INamedTypeSymbol? DeleteAttribute => this.deleteAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.DeleteAttribute));

        private INamedTypeSymbol? getAttribute;
        public INamedTypeSymbol? GetAttribute => this.getAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.GetAttribute));

        private INamedTypeSymbol? headAttribute;
        public INamedTypeSymbol? HeadAttribute => this.headAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.HeadAttribute));

        private INamedTypeSymbol? optionsAttribute;
        public INamedTypeSymbol? OptionsAttribute => this.optionsAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.OptionsAttribute));

        private INamedTypeSymbol? postAttribute;
        public INamedTypeSymbol? PostAttribute => this.postAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.PostAttribute));

        private INamedTypeSymbol? putAttribute;
        public INamedTypeSymbol? PutAttribute => this.putAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.PutAttribute));

        private INamedTypeSymbol? traceAttribute;
        public INamedTypeSymbol? TraceAttribute => this.traceAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.TraceAttribute));

        private INamedTypeSymbol? patchAttribute;
        public INamedTypeSymbol? PatchAttribute => this.patchAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.PatchAttribute));

        private INamedTypeSymbol? cancellationToken;
        public INamedTypeSymbol? CancellationToken => this.cancellationToken ??= this.LookupSystem("System.Threading.CancellationToken");

        private INamedTypeSymbol? task;
        public INamedTypeSymbol? Task => this.task ??= this.LookupSystem("System.Threading.Tasks.Task");

        private INamedTypeSymbol? taskT;
        public INamedTypeSymbol? TaskT => this.taskT ??= this.LookupSystem("System.Threading.Tasks.Task`1");

        private INamedTypeSymbol? ienumerableT;
        public INamedTypeSymbol? IEnumerableT => this.ienumerableT ??= this.LookupSystem("System.Collections.Generic.IEnumerable`1");

        private INamedTypeSymbol? idictionaryKV;
        public INamedTypeSymbol? IDictionaryKV => this.idictionaryKV ??= this.LookupSystem("System.Collections.Generic.IDictionary`2");

        private INamedTypeSymbol? httpMethod;
        public INamedTypeSymbol? HttpMethod => this.httpMethod ??= this.LookupSystem("System.Net.Http.HttpMethod");

        private INamedTypeSymbol? httpResponseMessage;
        public INamedTypeSymbol? HttpResponseMessage => this.httpResponseMessage ??= this.LookupSystem("System.Net.Http.HttpResponseMessage");

        private INamedTypeSymbol? stream;
        public INamedTypeSymbol? Stream => this.stream ??= this.LookupSystem("System.IO.Stream");

        private IMethodSymbol? idisposable_dispose;
        public IMethodSymbol? IDisposable_Dispose => this.idisposable_dispose ??= this.LookupSystem("System.IDisposable")?
            .GetMembers("Dispose").OfType<IMethodSymbol>().FirstOrDefault();

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
