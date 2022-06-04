using System;

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
        QueryMapParameterIsNotADictionary = 13,
        AllowAnyStatusCodeAttributeNotAllowedOnParentInterface = 14,
        EventsNotAllowed = 15,
        PropertyMustBeReadOnly = 16,
        MultipleRequesterProperties = 17,
        MethodMustHaveRequestAttribute = 18,
        MethodMustHaveValidReturnType = 19,
        PropertyMustHaveOneAttribute = 20,
        RequesterPropertyMustHaveZeroAttributes = 21,
        MultipleHttpRequestMessagePropertiesForKey = 22,
        HttpRequestMessageParamDuplicatesPropertyForKey = 23,
        MultipleHttpRequestMessageParametersForKey = 24,
        [Obsolete("No longer used")]
        ParameterMustHaveZeroOrOneAttributes = 25,
        CancellationTokenMustHaveZeroAttributes = 26,
        CouldNotFindRestEaseType = 27,
        CouldNotFindSystemType = 28,
        ExpressionsNotAvailable = 29,
        ParameterMustNotBeByRef = 30,
        InterfaceTypeMustBeAccessible = 31,
        AttributeConstructorNotRecognised = 32,
        AttributePropertyNotRecognised = 33,
        CouldNotFindRestEaseAssembly = 34,
        MissingPathPropertyForBaseAddressPlaceholder = 35,
        BaseAddressMustBeAbsolute = 36,
        RestEaseVersionTooOld = 37,
        RestEaseVersionTooNew = 38,
        MethodMustHaveOneRequestAttribute = 39,
        QueryConflictWithRawQueryString = 40,
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
