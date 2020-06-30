namespace RestEase.Implementation
{
    internal class ResolvedSerializationMethods
    {
        public const BodySerializationMethod DefaultBodySerializationMethod = BodySerializationMethod.Serialized;
        public const QuerySerializationMethod DefaultQuerySerializationMethod = QuerySerializationMethod.ToString;
        public const PathSerializationMethod DefaultPathSerializationMethod = PathSerializationMethod.ToString;

        public SerializationMethodsAttribute? ClassAttribute { get; private set; }

        public SerializationMethodsAttribute? MethodAttribute { get; private set; }


        public ResolvedSerializationMethods(SerializationMethodsAttribute? classAttribute, SerializationMethodsAttribute? methodAttribute)
        {
            this.ClassAttribute = classAttribute;
            this.MethodAttribute = methodAttribute;
        }

        public BodySerializationMethod ResolveBody(BodySerializationMethod parameterMethod)
        {
            if (parameterMethod != BodySerializationMethod.Default)
                return parameterMethod;

            if (this.MethodAttribute != null && this.MethodAttribute.Body != BodySerializationMethod.Default)
                return this.MethodAttribute.Body;

            if (this.ClassAttribute != null && this.ClassAttribute.Body != BodySerializationMethod.Default)
                return this.ClassAttribute.Body;

            return DefaultBodySerializationMethod;
        }

        public QuerySerializationMethod ResolveQuery(QuerySerializationMethod parameterMethod)
        {
            if (parameterMethod != QuerySerializationMethod.Default)
                return parameterMethod;

            if (this.MethodAttribute != null && this.MethodAttribute.Query != QuerySerializationMethod.Default)
                return this.MethodAttribute.Query;

            if (this.ClassAttribute != null && this.ClassAttribute.Query != QuerySerializationMethod.Default)
                return this.ClassAttribute.Query;

            return DefaultQuerySerializationMethod;
        }

        public PathSerializationMethod ResolvePath(PathSerializationMethod parameterMethod)
        {
            if (parameterMethod != PathSerializationMethod.Default)
                return parameterMethod;

            if (this.MethodAttribute != null && this.MethodAttribute.Path != PathSerializationMethod.Default)
                return this.MethodAttribute.Path;

            if (this.ClassAttribute != null && this.ClassAttribute.Path != PathSerializationMethod.Default)
                return this.ClassAttribute.Path;

            return DefaultPathSerializationMethod;
        }
    }
}
