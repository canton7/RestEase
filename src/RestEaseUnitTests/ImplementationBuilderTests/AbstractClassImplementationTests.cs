using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using RestEase;
using RestEase.Implementation;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class AbstractClassImplementationTests
    {
        public class Model
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public interface IRepository
        {
            Task<Model[]> ReadAll();
            Task<Model> ReadOne(int key);
            Task<int> Create(Model payload);
            Task Update(Model payload);
            Task Delete(int key);
        }

        public abstract class AbstractRestRepository: IRepository
        {
            [Get("/Entity")]
            public abstract Task<Model[]> ReadAll();
            [Get("/Entity/{id}")]
            public abstract Task<Model> ReadOne([Path]int id);
            [Post("/Entity")]
            public abstract Task<int> Create([Body]Model payload);
            [Put("/Entity")]
            public abstract Task Update([Body]Model payload);
            [Delete("/Entity/{id}")]
            public abstract Task Delete([Path]int id);
        }

        public abstract class InvalidAbstractRestRepository : IRepository
        {
            [Get("/Entity")]
            public abstract Task<Model[]> ReadAll();
            // missing attribute on purpose
            public abstract Task<Model> ReadOne(int key);
            public abstract Task<int> Create(Model payload);
            public abstract Task Update(Model payload);
            public abstract Task Delete(int key);
        }

        public abstract class InvalidAbstractRestRepository2 : IRepository
        {
            [Get("/Entity")]
            public abstract Task<Model[]> ReadAll();
            [Get("/Entity/{id}")]
            public abstract Task<Model> ReadOne(/* missing attribute on purpose */int id);
            [Post("/Entity")]
            public abstract Task<int> Create([Body]Model payload);
            [Put("/Entity")]
            public abstract Task Update([Body]Model payload);
            [Delete("/Entity/{id}")]
            public abstract Task Delete([Path]int id);
        }

        public abstract class ValidAbstractRestRepository : IRepository
        {
            [Get("/Entity")]
            public abstract Task<Model[]> ReadAll();
            [Get("/Entity/{id}")]
            public abstract Task<Model> ReadOne([Path]int id);
            [Post("/Entity")]
            public abstract Task<int> Create([Body]Model payload);
            public Task Update(Model payload)
            {
                return Task.Delay(0);
            }
            public Task Delete([Path] int id)
            {
                return Task.Delay(0);
            }
        }


        [Fact]
        public void ImplementsMethodsOfAnAbstractClass()
        {
            var requester = new Mock<IRequester>(MockBehavior.Strict);
            var builder = new ImplementationBuilder();
            // Does not throw
            var impl = builder.CreateImplementation<AbstractRestRepository>(requester.Object);

            var entities = new[] {new Model {Id = 1, Name = "First"}, new Model {Id = 2, Name = "Second"}};
            var newEntity = new Model {Name = "Third"};
            var updatedEntity = new Model {Id = 2, Name = "Updated Second"};

            var sequence = new MockSequence();
            requester.InSequence(sequence).Setup(x => x.RequestAsync<Model[]>(It.Is<IRequestInfo>(r => r.Method == HttpMethod.Get && r.Path == "/Entity" )))
                .Returns(Task.FromResult(entities));
            requester.InSequence(sequence).Setup(x => x.RequestAsync<Model>(It.Is<IRequestInfo>(r => r.Method == HttpMethod.Get && r.Path == "/Entity/{id}" && r.PathParams.Any(v => v.Key == "id" && v.Value == "1"))))
                .Returns(Task.FromResult(entities[0]));
            requester.InSequence(sequence).Setup(x => x.RequestAsync<int>(It.Is<IRequestInfo>(r => r.Method == HttpMethod.Post && r.Path == "/Entity" && r.BodyParameterInfo.ObjectValue == newEntity)))
                .Returns(Task.FromResult(3));
            requester.InSequence(sequence).Setup(x => x.RequestVoidAsync(It.Is<IRequestInfo>(r => r.Method == HttpMethod.Put && r.Path == "/Entity" && r.BodyParameterInfo.ObjectValue == updatedEntity)))
                .Returns(Task.Delay(0));
            requester.InSequence(sequence).Setup(x => x.RequestVoidAsync(It.Is<IRequestInfo>(r => r.Method == HttpMethod.Delete && r.Path == "/Entity/{id}" && r.PathParams.Any(v => v.Key == "id" && v.Value == "2"))))
                .Returns(Task.Delay(0));

            Assert.Equal(entities, impl.ReadAll().Result, new ModelComparer());
            Assert.Equal(entities[0], impl.ReadOne(1).Result, new ModelComparer());
            Assert.Equal(3, impl.Create(newEntity).Result);
            impl.Update(updatedEntity).Wait();
            impl.Delete(2).Wait();
        }

        [Fact]
        public void AllAbstractMethodsShouldBeImplemented()
        {
            Assert.Throws<ImplementationCreationException>(() =>
            {
                var requester = new Mock<IRequester>(MockBehavior.Strict);
                var builder = new ImplementationBuilder();
                builder.CreateImplementation<InvalidAbstractRestRepository>(requester.Object);
            });

            Assert.Throws<ImplementationCreationException>(() =>
            {
                var requester = new Mock<IRequester>(MockBehavior.Strict);
                var builder = new ImplementationBuilder();
                builder.CreateImplementation<InvalidAbstractRestRepository2>(requester.Object);
            });

            //Does not throw
            {
                var requester = new Mock<IRequester>(MockBehavior.Strict);
                var builder = new ImplementationBuilder();
                builder.CreateImplementation<ValidAbstractRestRepository>(requester.Object);
            }
        }

        private class ModelComparer : IEqualityComparer<Model>
        {
            public bool Equals(Model lhs, Model rhs)
            {
                return lhs.Id == rhs.Id && lhs.Name == rhs.Name;
            }

            public int GetHashCode(Model obj)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
