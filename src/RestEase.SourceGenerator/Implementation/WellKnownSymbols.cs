using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using RestEase.Implementation.Emission;

namespace RestEase.SourceGenerator.Implementation
{
    internal class WellKnownSymbols
    {
        private readonly Compilation compilation;
        private readonly IAssemblySymbol? restEaseAssembly;
        private readonly DiagnosticReporter diagnosticReporter;

        private INamedTypeSymbol? allowAnyStatusCodeAttribute;
        public INamedTypeSymbol? AllowAnyStatusCodeAttribute => this.allowAnyStatusCodeAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.AllowAnyStatusCodeAttribute));

        private INamedTypeSymbol? baseAddressAttribute;
        public INamedTypeSymbol? BaseAddressAttribute => this.baseAddressAttribute ??= this.LookupLocal("RestEase." + nameof(RestEase.BaseAddressAttribute));

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

        private INamedTypeSymbol? irequester;
        public INamedTypeSymbol? IRequester => this.irequester ??= this.LookupLocal("RestEase.IRequester");

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
        public INamedTypeSymbol? CancellationToken => this.cancellationToken ??= this.LookupKnownSystem("System.Threading.CancellationToken");

        private INamedTypeSymbol? task;
        public INamedTypeSymbol? Task => this.task ??= this.LookupKnownSystem("System.Threading.Tasks.Task");

        private INamedTypeSymbol? taskT;
        public INamedTypeSymbol? TaskT => this.taskT ??= this.LookupKnownSystem("System.Threading.Tasks.Task`1");

        private INamedTypeSymbol? ienumerableT;
        public INamedTypeSymbol? IEnumerableT => this.ienumerableT ??= this.LookupKnownSystem("System.Collections.Generic.IEnumerable`1");

        private INamedTypeSymbol? keyValuePairKV;
        public INamedTypeSymbol? KeyValuePairKV => this.keyValuePairKV ??= this.LookupKnownSystem("System.Collections.Generic.KeyValuePair`2");

        private INamedTypeSymbol? httpMethod;
        public INamedTypeSymbol? HttpMethod => this.httpMethod ??= this.LookupKnownSystem("System.Net.Http.HttpMethod");

        private INamedTypeSymbol? httpResponseMessage;
        public INamedTypeSymbol? HttpResponseMessage => this.httpResponseMessage ??= this.LookupKnownSystem("System.Net.Http.HttpResponseMessage");

        private INamedTypeSymbol? stream;
        public INamedTypeSymbol? Stream => this.stream ??= this.LookupKnownSystem("System.IO.Stream");

        private IMethodSymbol? idisposable_Dispose;
        public IMethodSymbol? IDisposable_Dispose => this.idisposable_Dispose ??= this.LookupKnownSystem("System.IDisposable")?
            .GetMembers("Dispose").OfType<IMethodSymbol>().FirstOrDefault();


        private bool? hasExpression;
        public bool HasExpression => this.hasExpression ??= this.GetHasExpression();

        public WellKnownSymbols(Compilation compilation, DiagnosticReporter diagnosticReporter)
        {
            this.compilation = compilation;
            this.diagnosticReporter = diagnosticReporter;
            var restEaseAssembly = GetReferencedAssemblies(compilation.Assembly)
                .FirstOrDefault(x => x.Name == "RestEase");
            if (restEaseAssembly == null)
            {
                this.diagnosticReporter.ReportCouldNotFindRestEaseAssembly();
            }
            else
            {
                var versionRangeAttribute = typeof(WellKnownSymbols).Assembly.GetCustomAttribute<AllowedRestEaseVersionRangeAttribute>();
                var (minInclusive, maxExclusive) = (new Version(versionRangeAttribute.MinVersionInclusive), new Version(versionRangeAttribute.MaxVersionExclusive));
                var restEaseVersion = restEaseAssembly.Identity.Version;

                if (restEaseVersion < minInclusive)
                {
                    this.diagnosticReporter.ReportRestEaseVersionTooOld(restEaseVersion, minInclusive, maxExclusive);
                }
                else if (restEaseVersion >= maxExclusive)
                {
                    this.diagnosticReporter.ReportRestEaseVersionTooNew(restEaseVersion, minInclusive, maxExclusive);
                }
                else
                {
                    this.restEaseAssembly = restEaseAssembly;
                }
            }
        }

        private static IEnumerable<IAssemblySymbol> GetReferencedAssemblies(IAssemblySymbol assembly)
        {
            return assembly.Modules.SelectMany(x => x.ReferencedAssemblySymbols);
        }

        private INamedTypeSymbol? LookupLocal(string metadataName)
        {
            if (this.restEaseAssembly == null)
                return null; // They've already got a diagnostic; they don't need more

            var type = this.restEaseAssembly.GetTypeByMetadataName(metadataName);
            if (type == null)
            {
                this.diagnosticReporter.ReportCouldNotFindRestEaseType(metadataName);
            }
            return type;
        }

        private INamedTypeSymbol? LookupKnownSystem(string metadataName)
        {
            if (this.restEaseAssembly == null)
                return null; // We simply can't generate anything in this case anyway

            // We search the assemblies that the RestEase assembly references. That way we can be fairly sure
            // that noone's created a type with the same name.
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

        private bool GetHasExpression()
        {
            var type = this.compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression");
            if (type != null)
            {
                return true;
            }

            this.diagnosticReporter.ReportExpressionsNotAvailable();
            return false;
        }
    }
}
