using Moq;
using RestEase.Implementation;
using System.Threading.Tasks;
using Xunit;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
#if !SOURCE_GENERATOR
    public class ThreadSafetyTests
    {
        public interface ISomeApi
        {
            [Get("foo")]
            Task GetFooAsync();
        }

        [Fact]
        public void CreateImplementationIsThreadSafe()
        {
            // Test passes if it does not throw "Duplicate type name within an assembly"

            var factory = new ImplementationFactory(useSourceGenerator: false, useSystemReflectionEmit: true);
            var requester = new Mock<IRequester>();

            // We can't really test this well... Just try lots, and see if we have any exceptions
            for (int i = 0; i < 100; i++)
            {
                var tasks = new Task[10];
                for (int j = 0; j < tasks.Length; j++)
                {
                    tasks[j] = Task.Run(() => factory.CreateImplementation<ISomeApi>(requester.Object));
                }

                Task.WaitAll(tasks);
            }
        }
    }
#endif
}
