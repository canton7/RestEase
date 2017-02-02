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

        public IEnumerable<IAttributedProperty> AllProperties
            => this.Headers.Concat<IAttributedProperty>(this.Path);

        public PropertyGrouping(IEnumerable<PropertyInfo> properties)
        {
            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite)
                    throw new ImplementationCreationException(String.Format("Property {0} must have both a getter and a setter", property.Name));

                var headerAttribute = property.GetCustomAttribute<HeaderAttribute>();
                if (headerAttribute != null)
                {
                    // Only allow default value if type is nullable (reference type or Nullable<T>)
                    if (headerAttribute.Value != null && property.PropertyType.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                        throw new ImplementationCreationException(String.Format("[Header(\"{0}\", \"{1}\")] on property {2} (i.e. containing a default value) can only be used if the property type is nullable", headerAttribute.Name, headerAttribute.Value, property.Name));
                    if (headerAttribute.Name.Contains(":"))
                        throw new ImplementationCreationException(String.Format("[Header(\"{0}\")] on property {1} must not have a colon in its name", headerAttribute.Name, property.Name));

                    this.Headers.Add(new AttributedProperty<HeaderAttribute>(headerAttribute, property));
                    continue;
                }

                var pathAttribute = property.GetCustomAttribute<PathAttribute>();
                if (pathAttribute != null)
                {
                    if (pathAttribute.Name == null)
                        pathAttribute.Name = property.Name;

                    this.Path.Add(new AttributedProperty<PathAttribute>(pathAttribute, property));
                    continue;
                }

                throw new ImplementationCreationException(String.Format("Property {0} does not have an attribute", property.Name));
            }
        }
    }
}
