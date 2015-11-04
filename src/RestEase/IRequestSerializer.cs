using System.Net.Http;
namespace RestEase
{
    /// <summary>
    /// Helper which knows how to serialize a request body and parameters
    /// </summary>
    public interface IRequestSerializer
    {
        /// <summary>
        /// Serialize the given query parameter value
        /// </summary>
        /// <typeparam name="T">Type of the query parameter value</typeparam>
        /// <param name="queryParameterValue">Query parameter value to serialize</param>
        /// <returns>Serialized value</returns>
        string SerializeQueryParameter<T>(T queryParameterValue);

        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <typeparam name="T">Type of the body to serialize</typeparam>
        /// <returns>HttpContent to assign to the request</returns>
        HttpContent SerializeBody<T>(T body);
    }
}
