using System.Net.Http;
namespace RestEase
{
    /// <summary>
    /// Helper which knows how to serialize a request body and parameters
    /// </summary>
    public interface IRequestSerializer
    {
        string SerializeQueryParameter<T>(T queryParameter);

        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <typeparam name="T">Type of the body to serialize</typeparam>
        /// <returns>HttpContent to assign to the request</returns>
        HttpContent SerializeBody<T>(T body);
    }
}
