using System;

namespace RestEase
{
    /// <summary>
    /// Encapsulates extra information provided to <see cref="RequestBodySerializer"/>
    /// </summary>
    /// <remarks>
    /// This is broken out as a separate structure so that extra properties can be added without breaking backwards compatibility
    /// </remarks>
    public readonly struct RequestBodySerializerInfo
    {
        /// <summary>
        /// Gets information about the request
        /// </summary>
        public IRequestInfo RequestInfo { get; }

        /// <summary>
        /// Gets the format provider. If this is null, the default will be used.
        /// Specified by the user on <see cref="RestClient.FormatProvider" />
        /// </summary>
        public IFormatProvider? FormatProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestBodySerializerInfo"/> structure
        /// </summary>
        /// <param name="requestInfo">Information about the request</param>
        /// <param name="formatProvider">Format provider to use</param>
        public RequestBodySerializerInfo(IRequestInfo requestInfo, IFormatProvider? formatProvider)
        {
            this.RequestInfo = requestInfo;
            this.FormatProvider = formatProvider;
        }
    }
}
