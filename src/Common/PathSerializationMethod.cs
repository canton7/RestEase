namespace RestEase
{
    /// <summary>
    /// Type of serialization that should be applied to the path parameter's value
    /// </summary>
    public enum PathSerializationMethod
    {
        /// <summary>
        /// Serialized using its .ToString() method
        /// </summary>
        ToString,

        /// <summary>
        /// Serialized using the configured RequestPathParamSerializer
        /// </summary>
        Serialized,

        /// <summary>
        /// Use the default serialization method. You probably don't want to specify this yourself
        /// </summary>
        Default,
    }
}
