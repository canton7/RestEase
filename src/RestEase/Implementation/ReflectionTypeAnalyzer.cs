using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
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
            var typeModel = new TypeModel(this.interfaceType)
            {
                SerializationMethodsAttribute = Get<SerializationMethodsAttribute>(),
                BaseAddressAttribute = Get<BaseAddressAttribute>(),
                BasePathAttribute = Get<BasePathAttribute>(),
                IsAccessible = IsAccessible(this.interfaceTypeInfo),
            };

            typeModel.HeaderAttributes.AddRange(GetAll<HeaderAttribute>());
            typeModel.AllowAnyStatusCodeAttributes.AddRange(GetAll<AllowAnyStatusCodeAttribute>());

            typeModel.Events.AddRange(this.InterfaceAndParents(x => x.GetEvents()).Select(x => EventModel.Instance));
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
                return attribute == null ? null : AttributeModel.Create(attribute, this.interfaceTypeInfo);
            }
            IEnumerable<AttributeModel<T>> GetAll<T>() where T : Attribute =>
                this.InterfaceAndParents().SelectMany(x => x.GetCustomAttributes<T>()
                    .Select(a => AttributeModel.Create(a, x)));
        }

        private static bool IsAccessible(TypeInfo queryTypeInfo)
        {
            // One of Public, NotPublic, NestedAssembly
            TypeAttributes? result = null;

            for (TypeInfo? typeInfo = queryTypeInfo; result == null && typeInfo != null; typeInfo = typeInfo!.DeclaringType?.GetTypeInfo())
            {
                var attributes = typeInfo.Attributes & TypeAttributes.VisibilityMask;
                switch (attributes)
                {
                    case TypeAttributes.NestedPublic:
                        break;

                    case TypeAttributes.Public:
                        result = TypeAttributes.Public;
                        break;

                    case TypeAttributes.NestedPrivate:
                    case TypeAttributes.NestedFamily:
                    case TypeAttributes.NestedFamANDAssem:
                        result = TypeAttributes.NotPublic;
                        break;

                    case TypeAttributes.NestedAssembly:
                    case TypeAttributes.NestedFamORAssem:
                    default: // Internal
                        result = TypeAttributes.NestedAssembly;
                        break;
                }
            }

            if (result == null)
            {
                // Not sure how we got here. Try and generate an implementation for it, which may fail (in which case we'll
                // produce another error message)
                Debug.Assert(false);
                result = TypeAttributes.Public;
            }
            else if (result == TypeAttributes.NestedAssembly)
            {
                if (queryTypeInfo.Assembly.GetCustomAttributes<InternalsVisibleToAttribute>().Any(x => x.AssemblyName == RestClient.FactoryAssemblyName))
                {
                    result = TypeAttributes.Public;
                }
            }

            return result == TypeAttributes.Public;
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
                return attribute == null ? null : AttributeModel.Create(attribute, propertyInfo);
            }
        }

        private MethodModel GetMethod(MethodInfo methodInfo)
        {
            var model = new MethodModel(methodInfo)
            {
                AllowAnyStatusCodeAttribute = Get<AllowAnyStatusCodeAttribute>(),
                SerializationMethodsAttribute = Get<SerializationMethodsAttribute>(),
                IsDisposeMethod = methodInfo == MethodInfos.IDisposable_Dispose,
            };
            model.RequestAttributes.AddRange(GetAll<RequestAttributeBase>());
            model.HeaderAttributes.AddRange(GetAll<HeaderAttribute>());

            model.Parameters.AddRange(methodInfo.GetParameters().Select(this.GetParameter));

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var attribute = methodInfo.GetCustomAttribute<T>();
                return attribute == null ? null : AttributeModel.Create(attribute, methodInfo);
            }
            IEnumerable<AttributeModel<T>> GetAll<T>() where T : Attribute =>
                methodInfo.GetCustomAttributes<T>().Select(x => AttributeModel.Create(x, methodInfo));
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
                IsByRef = parameterInfo.ParameterType.IsByRef,
            };

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var attribute = parameterInfo.GetCustomAttribute<T>();
                return attribute == null ? null : AttributeModel.Create(attribute, null);
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
