using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RestEase.Platform;

namespace RestEase.Implementation
{
    internal class PropertyGrouping
    {
        public List<AttributedProperty<HeaderAttribute>> Headers { get; } = new List<AttributedProperty<HeaderAttribute>>();
        public List<AttributedProperty<PathAttribute>> Path { get; } = new List<AttributedProperty<PathAttribute>>();
        public List<AttributedProperty<QueryAttribute>> Query { get; } = new List<AttributedProperty<QueryAttribute>>();

        public PropertyInfo Requester { get; private set; }

        public IEnumerable<IAttributedProperty> AllPropertiesWithStorage
            => this.Headers.Concat<IAttributedProperty>(this.Path).Concat(this.Query);

        public PropertyGrouping(IEnumerable<PropertyInfo> properties)
        {
            foreach (var property in properties)
            {
                var headerAttribute = property.GetCustomAttribute<HeaderAttribute>();
                if (headerAttribute != null)
                {
                    AssertHasGetterAndSetter(property);

                    // Only allow default value if type is nullable (reference type or Nullable<T>)
                    if (headerAttribute.Value != null && property.PropertyType.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                        throw new ImplementationCreationException($"[Header(\"{headerAttribute.Name}\", \"{headerAttribute.Value}\")] on property {property.Name} (i.e. containing a default value) can only be used if the property type is nullable");
                    if (headerAttribute.Name.Contains(":"))
                        throw new ImplementationCreationException($"[Header(\"{headerAttribute.Name}\")] on property {property.Name} must not have a colon in its name");

                    this.Headers.Add(new AttributedProperty<HeaderAttribute>(headerAttribute, property));
                    continue;
                }

                var pathAttribute = property.GetCustomAttribute<PathAttribute>();
                if (pathAttribute != null)
                {
                    AssertHasGetterAndSetter(property);

                    if (pathAttribute.Name == null)
                        pathAttribute.Name = property.Name;

                    this.Path.Add(new AttributedProperty<PathAttribute>(pathAttribute, property));
                    continue;
                }

                var queryAttribute = property.GetCustomAttribute<QueryAttribute>();
                if (queryAttribute != null)
                {
                    AssertHasGetterAndSetter(property);

                    if (queryAttribute.Name == null)
                        queryAttribute.Name = property.Name;

                    this.Query.Add(new AttributedProperty<QueryAttribute>(queryAttribute, property));
                    continue;
                }

                if (property.PropertyType == typeof(IRequester))
                {
                    if (!property.CanRead)
                        throw new ImplementationCreationException($"Property {property.Name} must have a getter");
                    if (property.CanWrite)
                        throw new ImplementationCreationException($"Property {property.Name} must not have a setter");
                    if (this.Requester != null)
                        throw new ImplementationCreationException($"Property {property.Name}: there must not be more than one property of type {nameof(IRequester)}");

                    this.Requester = property;
                    continue;
                }

                throw new ImplementationCreationException($"Property {property.Name} does not have an attribute");
            }
        }

        private static void AssertHasGetterAndSetter(PropertyInfo property)
        {
            if (!property.CanRead || !property.CanWrite)
                throw new ImplementationCreationException($"Property {property.Name} must have both a getter and a setter");
        }
    }
}
