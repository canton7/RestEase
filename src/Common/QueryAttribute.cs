using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as being a query param
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class QueryAttribute : Attribute
    {
        private string? _name;

        /// <summary>
        /// Gets or sets the name of the query param. Will use the parameter / property name if unset.
        /// </summary>
        public string? Name
        {
            get => this._name;
            set
            {
                this._name = value;
                this.HasName = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user has set the name attribute
        /// </summary>
        public bool HasName { get; private set; }

        /// <summary>
        /// Gets the serialization method to use to serialize the value. Defaults to QuerySerializationMethod.ToString
        /// </summary>
        public QuerySerializationMethod SerializationMethod { get; set; }

        /// <summary>
        /// Gets or sets the format string used to format the value
        /// </summary>
        /// <remarks>
        /// If <see cref="SerializationMethod"/> is <see cref="QuerySerializationMethod.Serialized"/>, this is passed to the serializer
        /// as <see cref="RequestQueryParamSerializerInfo.Format"/>.
        /// Otherwise, if this looks like a format string which can be passed to <see cref="string.Format(IFormatProvider, string, object[])"/>,
        /// (i.e. it contains at least one format placeholder), then this happens with the value passed as the first arg.
        /// Otherwise, if the value implements <see cref="IFormattable"/>, this is passed to the value's
        /// <see cref="IFormattable.ToString(string, IFormatProvider)"/> method. Otherwise this is ignored.
        /// Example values: "X2", "{0:X2}", "test{0}".
        /// </remarks>
        public string? Format { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute"/> class
        /// </summary>
        public QueryAttribute()
            : this(QuerySerializationMethod.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute"/> class, with the given serialization method
        /// </summary>
        /// <param name="serializationMethod">Serialization method to use to serialize the value</param>
        public QueryAttribute(QuerySerializationMethod serializationMethod)
        {
            // Don't set this.Name
            this.SerializationMethod = serializationMethod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute"/> class, with the given name
        /// </summary>
        /// <param name="name">Name of the query parameter</param>
        public QueryAttribute(string? name)
            : this(name, QuerySerializationMethod.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute"/> class, with the given name and serialization method
        /// </summary>
        /// <param name="name">Name of the query parameter</param>
        /// <param name="serializationMethod">Serialization method to use to serialize the value</param>
        public QueryAttribute(string? name, QuerySerializationMethod serializationMethod)
        {
            this.Name = name;
            this.SerializationMethod = serializationMethod;
        }
    }
}
