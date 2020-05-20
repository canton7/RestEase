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

        public RoslynTypeAnalyzer(INamedTypeSymbol namedTypeSymbol)
        {
            this.namedTypeSymbol = namedTypeSymbol;
        }

        public TypeModel Analyze()
        {
            var attributes = AttributeInstantiator.Instantiate(this.namedTypeSymbol);
            
            var typeModel = new TypeModel(this.namedTypeSymbol)
            {
                SerializationMethodsAttribute = Get<SerializationMethodsAttribute>(),
                BasePathAttribute = Get<BasePathAttribute>(),
            };

            var allowAnyStatusCodeAttributes = from type in this.InterfaceAndParents()
                                               let attributeData = type.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString(SymbolDisplayFormats.TypeLookup) == "RestEase.AllowAnyStatusCodeAttribute")
                                               where attributeData != null
                                               let instantiatedAttribute = AttributeInstantiator.Instantiate(attributeData) as AllowAnyStatusCodeAttribute
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
            var attributes = AttributeInstantiator.Instantiate(propertySymbol).ToList();

            var model = new PropertyModel(propertySymbol)
            {
                PathAttribute = Get<PathAttribute>(),
                QueryAttribute = Get<QueryAttribute>(),
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
            var attributes = AttributeInstantiator.Instantiate(methodSymbol).ToList();

            var model = new MethodModel(methodSymbol)
            {
                RequestAttribute = Get<RequestAttribute>(),
                AllowAnyStatusCodeAttribute = Get<AllowAnyStatusCodeAttribute>(),
            };

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
            var attributes = AttributeInstantiator.Instantiate(parameterSymbol).ToList();

            var model = new ParameterModel(parameterSymbol)
            {
                PathAttribute = Get<PathAttribute>(),
                QueryAttribute = Get<QueryAttribute>(),
                IsCancellationToken = parameterSymbol.Type.ToDisplayString(SymbolDisplayFormats.TypeLookup) == "System.Threading.CancellationToken",
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