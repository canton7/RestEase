namespace RestEase
{
    /// <summary>
    /// Encapsulates extra information provides to <see cref="RequestQueryParamSerializer"/>
    /// </summary>
    /// <remarks>
    /// This is broken out as a separate structure so that extra properties can be added without breaking backwards compatibility
    /// </remarks>
    public struct RequestQueryParamSerializerInfo
    {
        /// <summary>
        /// Gets information about the request
        /// </summary>
        public IRequestInfo RequestInfo { get; }

        /// <summary>
        /// Gets the format string specified using <see cref="QueryAttribute.Format"/>
        /// </summary>
        public string Format { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="RequestQueryParamSerializerInfo"/> structure
        /// </summary>
        /// <param name="requestInfo">Information about the request</param>
        /// <param name="format">Format string specified using <see cref="QueryAttribute.Format"/></param>
        public RequestQueryParamSerializerInfo(IRequestInfo requestInfo, string format)
        {
            this.RequestInfo = requestInfo;
            this.Format = format;
        }
    }
}
