namespace RestEase.Implementation
{
    /// <summary>
    /// Identifies the type of error / diagnostic encountered during emission
    /// </summary>
    public enum DiagnosticCode
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        None = 0,
        MultipleCancellationTokenParameters = 1,
        MissingPathPropertyForBasePathPlaceholder = 2,
        MissingPathPropertyOrParameterForPlaceholder = 3,
        MissingPlaceholderForPathParameter = 4,
        MultiplePathPropertiesForKey = 5,
        MultiplePathParametersForKey = 6,
        MultipleBodyParameters = 7,
        HeaderOnInterfaceMustHaveValue = 8,
        HeaderParameterMustNotHaveValue = 9,
        HeaderMustNotHaveColonInName = 10,
        PropertyMustBeReadWrite = 11,
        HeaderPropertyWithValueMustBeNullable = 12,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Extension methods on <see cref="DiagnosticCode"/>
    /// </summary>
    public static class DiagnosticCodeExtensions
    {
        /// <summary>
        /// Format the code as e.g. REST001
        /// </summary>
        public static string Format(this DiagnosticCode code)
        {
            return $"REST{(int)code:D3}";
        }
    }
}
