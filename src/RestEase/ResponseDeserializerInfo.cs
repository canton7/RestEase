namespace RestEase
{
    /// <summary>
    /// Encapsulates extra information provides to <see cref="ResponseDeserializer"/>
    /// </summary>
    /// <remarks>
    /// This is broken out as a separate structure so that extra properties can be added without breaking backwards compatibility
    /// </remarks>
    public struct ResponseDeserializerInfo
    {
        /// <summary>
        /// Gets information about the request
        /// </summary>
        public IRequestInfo RequestInfo { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="ResponseDeserializerInfo"/> structure
        /// </summary>
        /// <param name="requestInfo">Information about the request</param>
        public ResponseDeserializerInfo(IRequestInfo requestInfo)
        {
            this.RequestInfo = requestInfo;
        }
    }
}
