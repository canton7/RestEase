using System;

namespace RestEase
{
    /// <summary>
    /// Exception thrown if an interface implementation cannot be created
    /// </summary>
    public class ImplementationCreationException : Exception
    {
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
