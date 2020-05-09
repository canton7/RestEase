using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using RestEase.Implementation.Analysis;
using RestEase.Platform;

namespace RestEase.Implementation
{
    internal class ReflectionTypeAnalyzer
    {
        private readonly Type interfaceType;
        private readonly TypeInfo interfaceTypeInfo;

        public ReflectionTypeAnalyzer(Type interfaceType)
        {
            this.interfaceType = interfaceType;
            this.interfaceTypeInfo = interfaceType.GetTypeInfo();

            if (!this.interfaceTypeInfo.IsInterface)
                throw new ArgumentException(string.Format("Type {0} is not an interface", this.interfaceType.Name), nameof(interfaceType));
        }

        public TypeModel Analyze()
        {
            var typeModel = new TypeModel(this.interfaceType);

            var headerAttributes = this.InterfaceAndParents(x => x.GetCustomAttributes<HeaderAttribute>())
                .Select(x => AttributeModel.Create(x));
            typeModel.HeaderAttributes.AddRange(headerAttributes);

            var allowAnyStatusCodeAttributes = from type in this.InterfaceAndParents()
                                               let attribute = type.GetCustomAttribute<AllowAnyStatusCodeAttribute>()
                                               where attribute != null
                                               select new AllowAnyStatusCodeAttributeModel(attribute, type.AsType());
            typeModel.AllowAnyStatusCodeAttributes.AddRange(allowAnyStatusCodeAttributes);
            typeModel.Events.AddRange(this.InterfaceAndParents(x => x.GetEvents()).Select(x => EventModel.Instance));
            typeModel.SerializationMethodsAttribute = Get<SerializationMethodsAttribute>();
            typeModel.BasePathAttribute = Get<BasePathAttribute>();
            typeModel.Properties.AddRange(this.InterfaceAndParents(x => x.GetProperties()).Select(this.GetProperty));

            foreach (var methodInfo in this.InterfaceAndParents(x => x.GetMethods()))
            {
                // Exclude property getter / setters, etc
                if (!methodInfo.IsSpecialName)
                {
                    typeModel.Methods.Add(this.GetMethod(methodInfo));
                }
            }

            return typeModel;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var attribute = this.interfaceTypeInfo.GetCustomAttribute<T>();
                return attribute == null ? null : AttributeModel.Create(attribute);
            }
        }

        private PropertyModel GetProperty(PropertyInfo propertyInfo)
        {
            var model = new PropertyModel(propertyInfo)
            {
                HeaderAttribute = Get<HeaderAttribute>(),
                PathAttribute = Get<PathAttribute>(),
                QueryAttribute = Get<QueryAttribute>(),
                HttpRequestMessagePropertyAttribute = Get<HttpRequestMessagePropertyAttribute>(),
                IsRequester = propertyInfo.PropertyType == typeof(IRequester),
                HasGetter = propertyInfo.CanRead,
                HasSetter = propertyInfo.CanWrite,
            };
            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var attribute = propertyInfo.GetCustomAttribute<T>();
                return attribute == null ? null : AttributeModel.Create(attribute);
            }
        }

        private MethodModel GetMethod(MethodInfo methodInfo)
        {
            var model = new MethodModel(methodInfo)
            {
                RequestAttribute = Get<RequestAttribute>(),
                AllowAnyStatusCodeAttribute = Get<AllowAnyStatusCodeAttribute>(),
                SerializationMethodsAttribute = Get<SerializationMethodsAttribute>(),
                IsDisposeMethod = methodInfo == MethodInfos.IDisposable_Dispose,
            };
            model.HeaderAttributes.AddRange(methodInfo.GetCustomAttributes<HeaderAttribute>().Select(x => AttributeModel.Create(x)));

            model.Parameters.AddRange(methodInfo.GetParameters().Select(this.GetParameter));

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var attribute = methodInfo.GetCustomAttribute<T>();
                return attribute == null ? null : AttributeModel.Create(attribute);
            }
        }

        private ParameterModel GetParameter(ParameterInfo parameterInfo)
        {
            var model = new ParameterModel(parameterInfo)
            {
                HeaderAttribute = Get<HeaderAttribute>(),
                PathAttribute = Get<PathAttribute>(),
                QueryAttribute = Get<QueryAttribute>(),
                HttpRequestMessagePropertyAttribute = Get<HttpRequestMessagePropertyAttribute>(),
                RawQueryStringAttribute = Get<RawQueryStringAttribute>(),
                QueryMapAttribute = Get<QueryMapAttribute>(),
                BodyAttribute = Get<BodyAttribute>(),
                IsCancellationToken = parameterInfo.ParameterType == typeof(CancellationToken),
            };

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var attribute = parameterInfo.GetCustomAttribute<T>();
                return attribute == null ? null : AttributeModel.Create(attribute);
            }
        }

        private IEnumerable<TypeInfo> InterfaceAndParents()
        {
            var interfaceTypeInfo = this.interfaceType.GetTypeInfo();
            yield return interfaceTypeInfo;
            foreach (var parent in this.interfaceTypeInfo.GetInterfaces())
            {
               yield return parent.GetTypeInfo();
            }
        }

        private IEnumerable<T> InterfaceAndParents<T>(Func<TypeInfo, IEnumerable<T>> selector)
        {
            foreach (var item in selector(this.interfaceTypeInfo))
            {
                yield return item;
            }
            foreach (var parent in this.interfaceTypeInfo.GetInterfaces())
            {
                foreach (var item in selector(parent.GetTypeInfo()))
                {
                    yield return item;
                }
            }
        }
    }
}
