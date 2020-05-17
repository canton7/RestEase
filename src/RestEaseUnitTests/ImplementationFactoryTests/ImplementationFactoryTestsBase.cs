#if SOURCE_GENERATOR
extern alias SourceGenerator;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis;
using RestEase;
using SourceGenerator::RestEase.SourceGenerator;
using SourceGenerator::RestEase.SourceGenerator.Implementation;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;
using Moq;
using Xunit;
using Microsoft.CodeAnalysis.Emit;
using RestEase.Implementation;
using System.Diagnostics;
using System.Collections.Generic;
#else
using System;
using System.Threading.Tasks;
using Moq;
using RestEase;
using RestEase.Implementation;
using Xunit;
#endif

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public abstract class ImplementationFactoryTestsBase
    {
        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);

#if SOURCE_GENERATOR
        private static readonly Compilation compilation;
        private readonly RoslynImplementationFactory implementationFactory = new RoslynImplementationFactory();

        static ImplementationFactoryTestsBase()
        {
            string dotNetDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
            var project = new AdhocWorkspace()
                .AddProject("test", LanguageNames.CSharp)
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "netstandard.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "System.Runtime.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "System.Net.Http.dll")))
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(ImplementationFactoryTestsBase).Assembly.Location))
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(RestClient).Assembly.Location));
            compilation = project.GetCompilationAsync().Result;
        }

        protected T CreateImplementation<T>()
        {
            var namedTypeSymbol = compilation.GetTypeByMetadataName(typeof(T).FullName);
            var (sourceText, _) = this.implementationFactory.CreateImplementation(namedTypeSymbol);

            Assert.NotNull(sourceText);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText);
            var updatedCompilation = compilation.AddSyntaxTrees(syntaxTree);
            using (var peStream = new MemoryStream())
            {
                var emitResult = updatedCompilation.Emit(peStream);
                Assert.True(emitResult.Success, "Emit failed");
                var assembly = Assembly.Load(peStream.GetBuffer());
                var implementationType = assembly.GetCustomAttributes<RestEaseInterfaceImplementationAttribute>()
                    .FirstOrDefault(x => x.InterfaceType == typeof(T))?.ImplementationType;
                Assert.NotNull(implementationType);
                return (T)Activator.CreateInstance(implementationType, this.requester.Object);
            }
        }

        protected List<Diagnostic> GetDiagnostics<T>()
        {
            var namedTypeSymbol = compilation.GetTypeByMetadataName(typeof(T).FullName);
            var (_, diagnostics) = this.implementationFactory.CreateImplementation(namedTypeSymbol);
            return diagnostics;
        }

#else
        private readonly EmitImplementationFactory factory = EmitImplementationFactory.Instance;

        protected T CreateImplementation<T>()
        {
            return this.factory.CreateImplementation<T>(this.requester.Object);
        }
#endif

        protected IRequestInfo Request<T>(T implementation, Func<T, Task> method)
        {
            IRequestInfo requestInfo = null;
            var expectedResponse = Task.FromResult(false);

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(expectedResponse)
                .Verifiable();

            var response = method(implementation);

            Assert.Equal(expectedResponse, response);
            this.requester.Verify();

            return requestInfo;
        }


        protected IRequestInfo Request<T>(Func<T, Task> method)
        {
            return this.Request(this.CreateImplementation<T>(), method);
        }
    }
}
