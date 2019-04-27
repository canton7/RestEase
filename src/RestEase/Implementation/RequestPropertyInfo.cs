namespace RestEase.Implementation
{
    /// <summary>
    /// Structure containing information about a desired HTTP request message property
    /// </summary>
    public struct RequestPropertyInfo
    {
        /// <summary>
        /// Key of the key/value pair
        /// </summary>
        public string Key { get; private set; }
        /// <summary>
        /// Value of the key/value pair
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="RequestPropertyInfo"/> Structure
        /// </summary>
        /// <param name="key">Key of the key/value pair</param>
        /// <param name="value">Value of the key/value pair</param>
        public RequestPropertyInfo(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}