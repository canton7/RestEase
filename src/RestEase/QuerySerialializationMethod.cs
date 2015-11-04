namespace RestEase
{
    /// <summary>
    /// Type of serialization that should be applied to the query parameter's value
    /// </summary>
    public enum QuerySerialializationMethod
    {
        /// <summary>
        /// Serialized using its .ToString() method
        /// </summary>
        ToString,

        /// <summary>
        /// Serialized using the configured IRequestSerializer (uses Json.NET by default)
        /// </summary>
        Serialized
    }
}
