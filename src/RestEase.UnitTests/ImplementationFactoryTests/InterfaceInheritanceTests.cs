using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RestEase.Implementation;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class InterfaceInheritanceTests : ImplementationFactoryTestsBase
    {
        public interface IPropertyChild
        {
            [Header("X-Api-Token")]
            string ApiToken { get; set; }
        }

        public interface IPropertyParent : IPropertyChild
        {
            [Header("X-Api-Username")]
            string ApiUsername { get; set; }
        }

        public interface IMethodChild
        {
            [Get("/foo")]
            Task<string> GetFoo();
        }

        public interface IMethodParent : IMethodChild
        {
            [Get("/bar")]
            Task<string> GetBar();
        }

        [Header("X-Foo", "Bar")]
        public interface IChildWithInterfaceHeader
        {
        }

        [Header("X-Foo", "Baz")]
        public interface IParentWithInterfaceHeader : IChildWithInterfaceHeader
        {
            [Get("/foo")]
            Task FooAsync();
        }

        [AllowAnyStatusCode]
        public interface IChildWithAllowAnyStatusCode
        {
        }

        public interface IParentWithAllowAnyStatusCode : IChildWithAllowAnyStatusCode
        {
        }

        public interface IChildWithInvalidHeaderProperty
        {
            [Header("X-Foo:")]
            string Foo { get; set; }
        }

        public interface IParentWithInvalidHeaderProperty : IChildWithInvalidHeaderProperty
        {
        }

        public interface IChildWithEvent
        {
            event EventHandler Foo;
        }

        public interface IParentWithEvent : IChildWithEvent
        {
        }

        public interface ISameSignatureGenericB1
        {
            [Get]
            Task FooAsync<T1, T2>(T1 a, T2 b);
        }
        public interface ISameSignatureGenericB2
        {
            [Get]
            Task FooAsync<T1, T2>(T1 a, T2 b);
        }
        public interface ISameSignatureGenericB3
        {
            [Get]
            Task FooAsync<T1, T2>(T2 a, T1 b);
        }
        public interface IParentsSameSignatureGeneric : ISameSignatureGenericB1, ISameSignatureGenericB2, ISameSignatureGenericB3 { }

        public interface ISamePropertyNameDifferentTypesB1
        {
            [Query]
            string Foo { get; set; }
        }
        public interface ISamePropertyNameDifferentTypesB2
        {
            [Query]
            int Foo { get; set; }
        }
        public interface IParentsSamePropertyDifferentTypes : ISamePropertyNameDifferentTypesB1, ISamePropertyNameDifferentTypesB2
        {
            [Query]
            new double Foo { get; set; }

            [Get]
            Task FooAsync();
        }

        public interface IGenericBaseInterface<T>
        {
            [Query]
            T Foo { get; set; }

            [Get]
            Task FooAsync(T arg);
        }
        public interface IInheritsBaseTwiceDifferentTypeParams : IGenericBaseInterface<int>, IGenericBaseInterface<string>
        {
            [Get]
            new Task FooAsync(string arg);
        }

        public interface IDifferentReturnTypesBase
        {
            [Get]
            Task FooAsync();
        }
        public interface IDifferentReturnTypesOnSelfAndBase : IDifferentReturnTypesBase
        {
            [Get]
            new Task<string> FooAsync();
        }

        public interface IDifferentReturnTypesBase2
        {
            [Get]
            Task<string> FooAsync();
        }
        public interface IDifferentReturnTypesOnBases : IDifferentReturnTypesBase, IDifferentReturnTypesBase2 { }

        public InterfaceInheritanceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ImplementsPropertiesFromChild()
        {
            // Does not throw
            VerifyDiagnostics<IPropertyParent>();
        }

        [Fact]
        public void ImplementsMethodsFromChild()
        {
            // Does not throw
            VerifyDiagnostics<IMethodParent>();
        }

        [Fact]
        public void CombinesHeadersFromChildInterfaces()
        {
            var requestInfo = this.Request<IParentWithInterfaceHeader>(x => x.FooAsync());
            var classHeaders = requestInfo.ClassHeaders.ToList();

            Assert.Equal("X-Foo", classHeaders[0].Key);
            Assert.Equal("Baz", classHeaders[0].Value);

            Assert.Equal("X-Foo", classHeaders[1].Key);
            Assert.Equal("Bar", classHeaders[1].Value);
        }

        [Fact]
        public void DoesNotAllowAllowAnyStatusCodeOnChildInterfaces()
        {
            VerifyDiagnostics<IParentWithAllowAnyStatusCode>(
                // (-4,10): Error REST014: Parent interface (of tyoe 'IParentWithAllowAnyStatusCode') may not have an [AllowAnyStatusCode] attribute
                // AllowAnyStatusCode
                Diagnostic(DiagnosticCode.AllowAnyStatusCodeAttributeNotAllowedOnParentInterface, "AllowAnyStatusCode").WithLocation(-4, 10)
            );
        }

        [Fact]
        public void ValidatesHeadersOnChildProperties()
        {
            VerifyDiagnostics<IParentWithInvalidHeaderProperty>(
                // (-3,14): Error REST010: Header attribute name 'X-Foo:' must not contain a colon
                // Header("X-Foo:")
                Diagnostic(DiagnosticCode.HeaderMustNotHaveColonInName, @"Header(""X-Foo:"")").WithLocation(-3, 14)
            );
        }

        [Fact]
        public void ValidatesEventsInChildInterfaces()
        {
            VerifyDiagnostics<IParentWithEvent>(
                // (-2,32): Error REST015: Interfaces must not have any events
                // Foo,
                Diagnostic(DiagnosticCode.EventsNotAllowed, "Foo").WithLocation(-2, 32)
            );
        }

        [Fact]
        public void HandlesParentsSameSignatureGeneric()
        {
            var implementation = this.CreateImplementation<IParentsSameSignatureGeneric>();

            var methods = implementation.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.Contains(methods, x => x.Name.EndsWith("ISameSignatureGenericB1.FooAsync"));
            Assert.Contains(methods, x => x.Name.EndsWith("ISameSignatureGenericB2.FooAsync"));
            Assert.Contains(methods, x => x.Name == "FooAsync" && x.GetParameters()[0].ParameterType.Name == "T2" && x.GetParameters()[1].ParameterType.Name == "T1");
        }

        [Fact]
        public void HandlesParentsSamePropertyDifferentTypes()
        {
            var implementation = this.CreateImplementation<IParentsSamePropertyDifferentTypes>();

            implementation.Foo = 5.0;
            ((ISamePropertyNameDifferentTypesB1)implementation).Foo = "test";
            ((ISamePropertyNameDifferentTypesB2)implementation).Foo = 3;

            Assert.Equal(5.0, implementation.Foo);
            Assert.Equal("test", ((ISamePropertyNameDifferentTypesB1)implementation).Foo);
            Assert.Equal(3, ((ISamePropertyNameDifferentTypesB2)implementation).Foo);

            var requestInfo = this.Request(implementation, x => x.FooAsync());
            var queryProperties = requestInfo.QueryProperties.ToList();
            Assert.Equal(3, queryProperties.Count);

            var queryProperty0 = queryProperties[0].SerializeToString(null).ToList();
            Assert.Single(queryProperty0);
            Assert.Equal("Foo", queryProperty0[0].Key);
            Assert.Equal("5", queryProperty0[0].Value);

            var queryProperty1 = queryProperties[1].SerializeToString(null).ToList();
            Assert.Single(queryProperty1);
            Assert.Equal("Foo", queryProperty1[0].Key);
            Assert.Equal("test", queryProperty1[0].Value);

            var queryProperty2 = queryProperties[2].SerializeToString(null).ToList();
            Assert.Single(queryProperty2);
            Assert.Equal("Foo", queryProperty2[0].Key);
            Assert.Equal("3", queryProperty2[0].Value);

            var properties = implementation.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.Contains(properties, x => x.Name.EndsWith("ISamePropertyNameDifferentTypesB1.Foo"));
            Assert.Contains(properties, x => x.Name.EndsWith("ISamePropertyNameDifferentTypesB2.Foo"));
        }

        [Fact]
        public void HandlesGenericInterfaceInheritedTwiceWithDifferentTypeArguments()
        {
            var implementation = this.CreateImplementation<IInheritsBaseTwiceDifferentTypeParams>();

            var methods = implementation.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.Contains(methods, x => x.Name.EndsWith("IGenericBaseInterface<System.String>.FooAsync"));
            Assert.Contains(methods, x => x.Name == "FooAsync" && x.GetParameters()[0].ParameterType == typeof(string));
            Assert.Contains(methods, x => x.Name == "FooAsync" && x.GetParameters()[0].ParameterType == typeof(int));
        }

        [Fact]
        public void HandlesDifferentReturnTypesOnSelfAndBase()
        {
            var implementation = this.CreateImplementation<IDifferentReturnTypesOnSelfAndBase>();

            // The one on self gets implicitly implemented, the one on base is explicitly implemented
            var methods = implementation.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.Contains(methods, x => x.Name.EndsWith("IDifferentReturnTypesBase.FooAsync"));
            Assert.Contains(methods, x => x.Name == "FooAsync" && x.ReturnType == typeof(Task<string>));
        }

        [Fact]
        public void HandlesDifferentReturnTypesOnBases()
        {
            var implementation = this.CreateImplementation<IDifferentReturnTypesOnBases>();

            // If they're both on the bases, they both get explicitly implemented
            var methods = implementation.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.Contains(methods, x => x.Name.EndsWith("IDifferentReturnTypesBase.FooAsync"));
            Assert.Contains(methods, x => x.Name.EndsWith("IDifferentReturnTypesBase2.FooAsync"));
        }
    }
}
