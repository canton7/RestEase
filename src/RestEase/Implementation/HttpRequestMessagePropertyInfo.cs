namespace RestEase.Implementation
{
    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// Structure containing information about a desired HTTP request message property
    /// </summary>
    public struct HttpRequestMessagePropertyInfo
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
        /// Initialises a new instance of the <see cref="HttpRequestMessagePropertyInfo"/> Structure
        /// </summary>
        /// <param name="key">Key of the key/value pair</param>
        /// <param name="value">Value of the key/value pair</param>
        public HttpRequestMessagePropertyInfo(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}