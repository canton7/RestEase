#if SOURCE_GENERATOR
extern alias SourceGenerator;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis;
using RestEase;
using SourceGenerator::RestEase.SourceGenerator.Implementation;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;
using Moq;
using Xunit;
using RestEase.Implementation;
using System.Collections.Generic;
using RestEase.UnitTests.ImplementationFactoryTests.Helpers;
using Xunit.Abstractions;
using System.Linq.Expressions;
using System.Net.Http;
#else
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using RestEase;
using RestEase.Implementation;
using RestEase.UnitTests.ImplementationFactoryTests.Helpers;
using Xunit;
using Xunit.Abstractions;
#endif

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public abstract class ImplementationFactoryTestsBase
    {
        protected readonly Mock<IRequester> Requester = new(MockBehavior.Strict);
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ITestOutputHelper output;
#pragma warning restore IDE0052 // Remove unread private members

        public ImplementationFactoryTestsBase(ITestOutputHelper output)
        {
            this.output = output;
        }

#if SOURCE_GENERATOR
        private static readonly Compilation executionCompilation;
        private static readonly Compilation diagnosticsCompilation;

        static ImplementationFactoryTestsBase()
        {
            var thisAssembly = typeof(ImplementationFactoryTestsBase).Assembly;
            string dotNetDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);

            // For actually executing code, we need to reference the compiled test assembly, so that the types we're seeing at the same
            // types as the ones the unit tests are seeing.
            // However, this doesn't give us source locations, which we need in order to test diagnostics. So for testing these, we include
            // the test project's files as source, rather than referencing the test project.

            var executionProject = new AdhocWorkspace()
                .AddProject("Execution", LanguageNames.CSharp)
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new KeyValuePair<string, ReportDiagnostic>[]
                    {
                        KeyValuePair.Create("CS1701", ReportDiagnostic.Suppress),
                        KeyValuePair.Create("CS1702", ReportDiagnostic.Suppress),
                    })
                    .WithNullableContextOptions(NullableContextOptions.Enable))
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "netstandard.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "System.Runtime.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "System.Net.Http.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "System.Collections.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "System.Linq.Expressions.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(RestClient).Assembly.Location))
                .AddMetadataReference(MetadataReference.CreateFromFile(thisAssembly.Location));
            executionCompilation = executionProject.GetCompilationAsync().Result;

            var diagnosticsProject = new AdhocWorkspace()
                .AddProject("Diagnostics", LanguageNames.CSharp)
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithNullableContextOptions(NullableContextOptions.Enable))
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "netstandard.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "System.Runtime.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "System.Net.Http.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "System.Linq.Expressions.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(RestClient).Assembly.Location));
            diagnosticsCompilation = diagnosticsProject.GetCompilationAsync().Result;

            var syntaxTrees = new List<SyntaxTree>();
            foreach (string resourceName in thisAssembly.GetManifestResourceNames())
            {
                if (resourceName.EndsWith(".cs"))
                {
                    using (var reader = new StreamReader(thisAssembly.GetManifestResourceStream(resourceName)))
                    {
                        syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(reader.ReadToEnd()));
                    }
                }
            }

            diagnosticsCompilation = diagnosticsCompilation.AddSyntaxTrees(syntaxTrees);
        }

        protected T CreateImplementation<T>()
        {
            var typeToSearch = typeof(T).IsGenericType ? typeof(T).GetGenericTypeDefinition() : typeof(T);
            string metadataName = typeToSearch.FullName;

            var factory = new RoslynImplementationFactory(executionCompilation);
            var namedTypeSymbol = executionCompilation.GetTypeByMetadataName(metadataName);
            var (sourceText, diagnostics) = factory.CreateImplementation(namedTypeSymbol);

            var allDiagnostics = diagnostics.Concat(factory.GetCompilationDiagnostics()).ToList();
            Assert.True(allDiagnostics.Count == 0, "Unexpected diagnostics:\r\n\r\n" + string.Join("\r\n", allDiagnostics.Select(x => x.ToString())));
            Assert.NotNull(sourceText); // Just in case there are no diagnostics

            this.output.WriteLine(sourceText.ToString());

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText);
            var updatedCompilation = executionCompilation.AddSyntaxTrees(syntaxTree);
            using (var peStream = new MemoryStream())
            {
                var emitResult = updatedCompilation.Emit(peStream);

                Assert.True(emitResult.Diagnostics.Length == 0, "Unexpected compiler diagnostics:\r\n\r\n" + string.Join("\r\n", emitResult.Diagnostics.Select(x => x.ToString())));
                Assert.True(emitResult.Success, "Emit failed"); // Just in case it failed without diagnostics...
                var assembly = Assembly.Load(peStream.GetBuffer());
                var implementationType = assembly.GetCustomAttributes<RestEaseInterfaceImplementationAttribute>()
                    .FirstOrDefault(x => x.InterfaceType == typeToSearch)?.ImplementationType;
                Assert.NotNull(implementationType);
                if (typeof(T).IsGenericType)
                {
                    implementationType = implementationType.MakeGenericType(typeof(T).GetGenericArguments());
                }
                return (T)Activator.CreateInstance(implementationType, this.Requester.Object);
            }
        }

        protected static void VerifyDiagnostics<T>(params DiagnosticResult[] expected)
        {
            var namedTypeSymbol = diagnosticsCompilation.GetTypeByMetadataName(typeof(T).FullName);

            var factory = new RoslynImplementationFactory(diagnosticsCompilation);
            var (_, diagnostics) = factory.CreateImplementation(namedTypeSymbol);
            int lineOffset = namedTypeSymbol.DeclaringSyntaxReferences[0].GetSyntax().GetLocation().GetLineSpan().StartLinePosition.Line;
            DiagnosticVerifier.VerifyDiagnostics(diagnostics.Concat(factory.GetCompilationDiagnostics()), expected, lineOffset);
        }

#else
        private readonly ImplementationFactory factory = new(useSourceGenerator: false);

        protected T CreateImplementation<T>()
        {
            return this.factory.CreateImplementation<T>(this.Requester.Object);
        }

        protected void VerifyDiagnostics<T>(params DiagnosticResult[] expected)
        {
            if (expected.Length == 0)
            {
                // Check doesn't throw
                this.CreateImplementation<T>();
            }
            else
            {
                var ex = Assert.Throws<ImplementationCreationException>(() => this.CreateImplementation<T>());
                if (!expected.Any(x => x.Code == ex.Code))
                {
                    Assert.Equal(expected[0].Code, ex.Code);
                }
            }
        }
#endif

        protected static DiagnosticResult Diagnostic(DiagnosticCode code, string squiggledText)
        {
            return new DiagnosticResult(code, squiggledText);
        }

        protected IRequestInfo Request<TType>(
            TType implementation,
            Func<TType, Task> method,
            Expression<Func<IRequester, Task>> requesterMethod)
        {
            IRequestInfo requestInfo = null;
            var returnValue = Task.FromResult(false);

            this.Requester.Setup(requesterMethod)
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(returnValue)
                .Verifiable();

            var response = method(implementation);

            Assert.Equal(returnValue, response);
            this.Requester.Verify();

            return requestInfo;
        }

        private IRequestInfo Request<TType, TReturnType>(
            TType implementation,
            Func<TType, Task<TReturnType>> method,
            Expression<Func<IRequester, Task<TReturnType>>> requesterMethod,
            TReturnType returnValue)
        {
            IRequestInfo requestInfo = null;

            this.Requester.Setup(requesterMethod)
                .Callback((IRequestInfo r) => requestInfo = r)
                .ReturnsAsync(returnValue)
                .Verifiable();

            var response = method(implementation);

            Assert.Equal(returnValue, response.Result);
            this.Requester.Verify();

            return requestInfo;
        }

        protected IRequestInfo Request<T>(T implementation, Func<T, Task> method)
        {
            return this.Request(implementation, method, x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()));
        }

        protected IRequestInfo Request<T>(Func<T, Task> method)
        {
            return this.Request(this.CreateImplementation<T>(), method);
        }

        protected IRequestInfo Request<TType, TReturnType>(Func<TType, Task<TReturnType>> method, TReturnType returnValue)
        {
            return this.Request(this.CreateImplementation<TType>(), method, x => x.RequestAsync<TReturnType>(It.IsAny<IRequestInfo>()), returnValue);
        }

        protected IRequestInfo RequestWithResponse<TType, TReturnType>(Func<TType, Task<Response<TReturnType>>> method, Response<TReturnType> returnValue)
        {
            return this.Request(this.CreateImplementation<TType>(), method, x => x.RequestWithResponseAsync<TReturnType>(It.IsAny<IRequestInfo>()), returnValue);
        }

        protected IRequestInfo RequestWithResponseMessage<TType>(Func<TType, Task<HttpResponseMessage>> method, HttpResponseMessage returnValue)
        {
            return this.Request(this.CreateImplementation<TType>(), method, x => x.RequestWithResponseMessageAsync(It.IsAny<IRequestInfo>()), returnValue);
        }

        protected IRequestInfo RequestRaw<TType>(Func<TType, Task<string>> method, string returnValue)
        {
            return this.Request(this.CreateImplementation<TType>(), method, x => x.RequestRawAsync(It.IsAny<IRequestInfo>()), returnValue);
        }

        protected IRequestInfo RequestStream<TType>(Func<TType, Task<Stream>> method, Stream returnValue)
        {
            return this.Request(this.CreateImplementation<TType>(), method, x => x.RequestStreamAsync(It.IsAny<IRequestInfo>()), returnValue);
        }
    }
}
