using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.CodeAnalysis;
using RestEase.Implementation.Analysis;

namespace RestEase.SourceGenerator.Implementation
{
    internal class RoslynTypeAnalyzer
    {
        private readonly INamedTypeSymbol namedTypeSymbol;
        private readonly WellKnownSymbols wellKnownSymbols;
        private readonly AttributeInstantiator attributeInstantiator;

        public RoslynTypeAnalyzer(INamedTypeSymbol namedTypeSymbol, WellKnownSymbols wellKnownSymbols)
        {
            this.namedTypeSymbol = namedTypeSymbol;
            this.wellKnownSymbols = wellKnownSymbols;
            this.attributeInstantiator = new AttributeInstantiator(wellKnownSymbols);
        }

        public TypeModel Analyze()
        {
            var attributes = this.attributeInstantiator.Instantiate(this.namedTypeSymbol);
            
            var typeModel = new TypeModel(this.namedTypeSymbol)
            {
                SerializationMethodsAttribute = Get<SerializationMethodsAttribute>(),
                BasePathAttribute = Get<BasePathAttribute>(),
            };

            var headerAttributes = from type in this.InterfaceAndParents()
                                   let attributeDatas = type.GetAttributes()
                                       .Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, this.wellKnownSymbols.HeaderAttribute))
                                   from attributeData in attributeDatas
                                   let instantiatedAttribute = this.attributeInstantiator.Instantiate(attributeData) as HeaderAttribute
                                   where instantiatedAttribute != null
                                   select AttributeModel.Create(instantiatedAttribute, attributeData);

            typeModel.HeaderAttributes.AddRange(headerAttributes);

            var allowAnyStatusCodeAttributes = from type in this.InterfaceAndParents()
                                               let attributeData = type.GetAttributes()
                                                   .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, this.wellKnownSymbols.AllowAnyStatusCodeAttribute))
                                               where attributeData != null
                                               let instantiatedAttribute = this.attributeInstantiator.Instantiate(attributeData) as AllowAnyStatusCodeAttribute
                                               where instantiatedAttribute != null
                                               select new AllowAnyStatusCodeAttributeModel(instantiatedAttribute, type, attributeData);
            typeModel.AllowAnyStatusCodeAttributes.AddRange(allowAnyStatusCodeAttributes);

            foreach (var member in this.namedTypeSymbol.GetMembers())
            {
                switch (member)
                {
                    case IPropertySymbol property:
                        typeModel.Properties.Add(this.GetProperty(property));
                        break;
                    case IMethodSymbol method when method.MethodKind == MethodKind.Ordinary:
                        typeModel.Methods.Add(this.GetMethod(method));
                        break;
                }
            }

            return typeModel;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var (attribute, attributeData) = attributes.FirstOrDefault(x => x.attribute is T);
                return attribute == null ? null : AttributeModel.Create((T)attribute, attributeData);
            }
        }

        private PropertyModel GetProperty(IPropertySymbol propertySymbol)
        {
            var attributes = this.attributeInstantiator.Instantiate(propertySymbol).ToList();

            var model = new PropertyModel(propertySymbol)
            {
                HeaderAttribute = Get<HeaderAttribute>(),
                PathAttribute = Get<PathAttribute>(),
                QueryAttribute = Get<QueryAttribute>(),
                HttpRequestMessagePropertyAttribute = Get<HttpRequestMessagePropertyAttribute>(),
                HasGetter = propertySymbol.GetMethod != null,
                HasSetter = propertySymbol.SetMethod != null,
            };

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var (attribute, attributeData) = attributes.FirstOrDefault(x => x.attribute is T);
                return attribute == null ? null : AttributeModel.Create((T)attribute, attributeData);
            }
        }

        private MethodModel GetMethod(IMethodSymbol methodSymbol)
        {
            var attributes = this.attributeInstantiator.Instantiate(methodSymbol).ToList();

            var model = new MethodModel(methodSymbol)
            {
                RequestAttribute = Get<RequestAttribute>(),
                AllowAnyStatusCodeAttribute = Get<AllowAnyStatusCodeAttribute>(),
                SerializationMethodsAttribute = Get<SerializationMethodsAttribute>(),
            };
            model.HeaderAttributes.AddRange(attributes.Where(x => x.attribute is HeaderAttribute)
                .Select(x => AttributeModel.Create((HeaderAttribute)x.attribute!, x.attributeData)));

            model.Parameters.AddRange(methodSymbol.Parameters.Select(this.GetParameter));

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var (attribute, attributeData) = attributes.FirstOrDefault(x => x.attribute is T);
                return attribute == null ? null : AttributeModel.Create((T)attribute, attributeData);
            }
        }

        private ParameterModel GetParameter(IParameterSymbol parameterSymbol)
        {
            var attributes = this.attributeInstantiator.Instantiate(parameterSymbol).ToList();

            var model = new ParameterModel(parameterSymbol)
            {
                HeaderAttribute = Get<HeaderAttribute>(),
                PathAttribute = Get<PathAttribute>(),
                QueryAttribute = Get<QueryAttribute>(),
                HttpRequestMessagePropertyAttribute = Get<HttpRequestMessagePropertyAttribute>(),
                RawQueryStringAttribute = Get<RawQueryStringAttribute>(),
                QueryMapAttribute = Get<QueryMapAttribute>(),
                BodyAttribute = Get<BodyAttribute>(),
                IsCancellationToken = SymbolEqualityComparer.Default.Equals(parameterSymbol.Type, this.wellKnownSymbols.CancellationToken),
            };

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var (attribute, attributeData) = attributes.FirstOrDefault(x => x.attribute is T);
                return attribute == null ? null : AttributeModel.Create((T)attribute, attributeData);
            }
        }

        private IEnumerable<INamedTypeSymbol> InterfaceAndParents()
        {
            yield return this.namedTypeSymbol;
            foreach (var parent in this.namedTypeSymbol.AllInterfaces)
            {
                yield return parent;
            }
        }
    }
}