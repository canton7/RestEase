namespace RestEase
{
    /// <summary>
    /// Encapsulates extra information provides to <see cref="IRequestQueryParamSerializer"/>
    /// </summary>
    /// <remarks>
    /// This is broken out as a separate structure so that extra properties can be added without breaking backwards compatibility
    /// </remarks>
    public struct RequestQueryParamSerializerInfo
    {
        /// <summary>
        /// Gets the format string specified using <see cref="QueryAttribute.Format"/>
        /// </summary>
        public string Format { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="RequestQueryParamSerializerInfo"/> structure
        /// </summary>
        /// <param name="format">Format string specified using <see cref="QueryAttribute.Format"/></param>
        public RequestQueryParamSerializerInfo(string format)
        {
            this.Format = format;
        }
    }
}
