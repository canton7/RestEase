using System;

namespace RestEase.Implementation
{
    /// <summary>
    /// Exception thrown if an interface implementation cannot be created
    /// </summary>
    public class ImplementationCreationException : Exception
    {
        /// <summary>
        /// Gets the code of this error
        /// </summary>
        public DiagnosticCode Code { get; } = DiagnosticCode.None;

        /// <summary>
        /// Initialises a new instance of the <see cref="ImplementationCreationException"/> class
        /// </summary>
        /// <param name="code">Code of this error</param>
        /// <param name="message">Message to use</param>
        public ImplementationCreationException(DiagnosticCode code, string message)
            : base(message)
        {
            this.Code = code;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ImplementationCreationException"/> class
        /// </summary>
        /// <param name="message">Message to use</param>
        public ImplementationCreationException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="ImplementationCreationException"/> class
        /// </summary>
        /// <param name="message">Message to use</param>
        /// <param name="innerException">InnerException to use</param>
        public ImplementationCreationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
