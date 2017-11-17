namespace RestEase
{
    /// <summary>
    /// Encapsulates extra information provided to <see cref="RequestBodySerializer"/>
    /// </summary>
    /// <remarks>
    /// This is broken out as a separate structure so that extra properties can be added without breaking backwards compatibility
    /// </remarks>
    public struct RequestBodySerializerInfo
    {
        /// <summary>
        /// Gets information about the request
        /// </summary>
        public IRequestInfo RequestInfo { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="RequestBodySerializerInfo"/> structure
        /// </summary>
        /// <param name="requestInfo">Information about the request</param>
        public RequestBodySerializerInfo(IRequestInfo requestInfo)
        {
            this.RequestInfo = requestInfo;
        }
    }
}
