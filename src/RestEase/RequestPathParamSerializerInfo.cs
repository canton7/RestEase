using System;

namespace RestEase
{
    /// <summary>
    /// Encapsulates extra information provides to <see cref="RequestPathParamSerializer"/>
    /// </summary>
    /// <remarks>
    /// This is broken out as a separate structure so that extra properties can be added without breaking backwards compatibility
    /// </remarks>
    public readonly struct RequestPathParamSerializerInfo
    {
        /// <summary>
        /// Gets information about the request
        /// </summary>
        public IRequestInfo RequestInfo { get; }

        /// <summary>
        /// Gets the format string specified using <see cref="PathAttribute.Format"/>
        /// </summary>
        public string? Format { get; }

        /// <summary>
        /// Gets the format provider. If this is null, the default will be used.
        /// Specified by the user on <see cref="RestClient.FormatProvider" />
        /// </summary>
        public IFormatProvider? FormatProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestPathParamSerializerInfo"/> structure
        /// </summary>
        /// <param name="requestInfo">Information about the request</param>
        /// <param name="format">Format string specified using <see cref="PathAttribute.Format"/></param>
        /// <param name="formatProvider">Format provider to use</param>
        public RequestPathParamSerializerInfo(IRequestInfo requestInfo, string? format, IFormatProvider? formatProvider)
        {
            this.RequestInfo = requestInfo;
            this.Format = format;
            this.FormatProvider = formatProvider;
        }
    }
}
