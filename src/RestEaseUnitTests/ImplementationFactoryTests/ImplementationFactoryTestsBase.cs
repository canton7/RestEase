#if SOURCE_GENERATOR
extern alias SourceGenerator;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis;
using RestEase;
using SourceGenerator::RestEase.SourceGenerator;
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
#if SOURCE_GENERATOR
        private static readonly IAssemblySymbol assembly;
        private readonly RoslynImplementationFactory implementationFactory = new RoslynImplementationFactory();

        static ImplementationFactoryTestsBase()
        {
            var project = new AdhocWorkspace()
                .AddProject("test", LanguageNames.CSharp)
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(ImplementationFactoryTestsBase).Assembly.Location));
            var compilation = project.GetCompilationAsync().Result;
            // We know there's only one MetadataReference
            assembly = compilation.Assembly.Modules.Single().ReferencedAssemblySymbols.Single();
        }

        protected T CreateImplementation<T>()
        {
            var namedTypeSymbol = assembly.GetTypeByMetadataName(typeof(T).FullName);
            this.implementationFactory.CreateImplementation(namedTypeSymbol);

            throw new NotSupportedException();
        }

        protected IRequestInfo Request<T>(T implementation, Func<T, Task> method)
        {
            throw new NotSupportedException();
        }
#else
        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly EmitImplementationFactory factory = EmitImplementationFactory.Instance;

        protected T CreateImplementation<T>()
        {
            return this.factory.CreateImplementation<T>(this.requester.Object);
        }

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
#endif

        protected IRequestInfo Request<T>(Func<T, Task> method)
        {
            return this.Request(this.CreateImplementation<T>(), method);
        }
    }
}
