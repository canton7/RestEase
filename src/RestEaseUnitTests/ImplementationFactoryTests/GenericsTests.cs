using Moq;
using RestEase;
using RestEase.Implementation;
using RestEaseUnitTests.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class GenericsTests
    {
        public interface IHasGenericMethod
        {
            [Get("foo")]
            Task Foo<T>();
        }

        public interface IHasGenericReturnType
        {
            [Get("foo")]
            Task<T> Foo<T>();
        }

        public interface IHasGenericResponseReturnType
        {
            [Get("foo")]
            Task<Response<T>> Foo<T>();
        }

        public interface IHasGenericParameters
        {
            [Get("foo/{a}")]
            Task Foo<T1, T2>([Path] T1 a, [Query] T1 b, [Query] T2 c, [Query] IEnumerable<T1> d);
        }

        public class Base { }
        public interface IInterface { }
        public interface IHasGenericConstraint
        {
            [Get("foo")]
            Task Foo<T>() where T : Base, IInterface, new();
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly EmitImplementationFactory factory = EmitImplementationFactory.Instance;

        [Fact]
        public void SupportsGenericMethods()
        {
            this.factory.CreateImplementation<IHasGenericMethod>(this.requester.Object);
        }

        [Fact]
        public void SupportsGenericReturnType()
        {
            var implementation = this.factory.CreateImplementation<IHasGenericReturnType>(this.requester.Object);

            IRequestInfo requestInfo = null;
            this.requester.Setup(x => x.RequestAsync<string>(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult("blah"));

            Assert.Equal("blah", implementation.Foo<string>().Result);
        }

        [Fact]
        public void SupportsGenericResponseReturnType()
        {
            var implementation = this.factory.CreateImplementation<IHasGenericResponseReturnType>(this.requester.Object);

            IRequestInfo requestInfo = null;
            this.requester.Setup(x => x.RequestWithResponseAsync<string>(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(new Response<string>("blah", null, null)));

            Assert.Equal("blah", implementation.Foo<string>().Result.StringContent);
        }

        [Fact]
        public void SupportsGenericMethodParameters()
        {
            var implementation = this.factory.CreateImplementation<IHasGenericParameters>(this.requester.Object);

            IRequestInfo requestInfo = null;
            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.Foo("foo", "bar", new[] { "a", "b" }, new[] { "c", "d" });

            var pathParams = requestInfo.PathParams.ToArray();
            Assert.Single(pathParams);
            Assert.Equal("a", pathParams[0].SerializeToString(null).Key);
            Assert.Equal("foo", pathParams[0].SerializeToString(null).Value);

            var queryParams = requestInfo.QueryParams.ToArray();
            Assert.Equal(3, queryParams.Length);

            Assert.Equal("b", queryParams[0].SerializeToString(null).ToArray()[0].Key);
            Assert.Equal("bar", queryParams[0].SerializeToString(null).ToArray()[0].Value);

            Assert.Equal("c", queryParams[1].SerializeToString(null).ToArray()[0].Key);
            Assert.Equal("System.String[]", queryParams[1].SerializeToString(null).ToArray()[0].Value);

            Assert.Equal("d", queryParams[2].SerializeToString(null).ToArray()[0].Key);
            Assert.Equal("c", queryParams[2].SerializeToString(null).ToArray()[0].Value);
            Assert.Equal("d", queryParams[2].SerializeToString(null).ToArray()[1].Key);
            Assert.Equal("d", queryParams[2].SerializeToString(null).ToArray()[1].Value);
        }

        [Fact]
        public void SupportsGenericConstraints()
        {
            var implementation = this.factory.CreateImplementation<IHasGenericConstraint>(this.requester.Object);
            var methodInfo = implementation.GetType().GetMethod("Foo");
            Assert.Equal(new[] { typeof(IInterface), typeof(Base) }, methodInfo.GetGenericArguments()[0].GetGenericParameterConstraints());
            Assert.Equal(GenericParameterAttributes.DefaultConstructorConstraint, methodInfo.GetGenericArguments()[0].GetGenericParameterAttributes());
        }
    }
}
