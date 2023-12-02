using System;

namespace RestEase
{
    /// <summary>
    /// Encapsulates extra information provides to <see cref="RequestQueryParamSerializer"/>
    /// </summary>
    /// <remarks>
    /// This is broken out as a separate structure so that extra properties can be added without breaking backwards compatibility
    /// </remarks>
    public readonly struct RequestQueryParamSerializerInfo
    {
        /// <summary>
        /// Gets information about the request
        /// </summary>
        public IRequestInfo RequestInfo { get; }

        /// <summary>
        /// Gets the format string specified using <see cref="QueryAttribute.Format"/>
        /// </summary>
        public string? Format { get; }

        /// <summary>
        /// Gets the format provider. If this is null, the default will be used.
        /// Specified by the user on <see cref="RestClient.FormatProvider" />
        /// </summary>
        public IFormatProvider? FormatProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestQueryParamSerializerInfo"/> structure
        /// </summary>
        /// <param name="requestInfo">Information about the request</param>
        /// <param name="format">Format string specified using <see cref="QueryAttribute.Format"/></param>
        /// <param name="formatProvider">Format provider to use</param>
        public RequestQueryParamSerializerInfo(IRequestInfo requestInfo, string? format, IFormatProvider? formatProvider)
        {
            this.RequestInfo = requestInfo;
            this.Format = format;
            this.FormatProvider = formatProvider;
        }
    }
}
