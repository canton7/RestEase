using System;
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
            };

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
                var attribute = (T?)attributes.FirstOrDefault(x => x is T);
                return attribute == null ? null : AttributeModel.Create(attribute);
            }
        }

        private PropertyModel GetProperty(IPropertySymbol propertySymbol)
        {
            var attributes = AttributeInstantiator.Instantiate(propertySymbol).ToList();

            var model = new PropertyModel(propertySymbol)
            {
                QueryAttribute = Get<QueryAttribute>(),
                HasGetter = propertySymbol.GetMethod != null,
                HasSetter = propertySymbol.SetMethod != null,
            };

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var attribute = (T?)attributes.FirstOrDefault(x => x is T);
                return attribute == null ? null : AttributeModel.Create(attribute);
            }
        }

        private MethodModel GetMethod(IMethodSymbol methodSymbol)
        {
            var attributes = AttributeInstantiator.Instantiate(methodSymbol).ToList();

            var model = new MethodModel(methodSymbol)
            {
                RequestAttribute = Get<RequestAttribute>(),
            };

            model.Parameters.AddRange(methodSymbol.Parameters.Select(this.GetParameter));

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var attribute = (T?)attributes.FirstOrDefault(x => x is T);
                return attribute == null ? null : AttributeModel.Create(attribute);
            }
        }

        private ParameterModel GetParameter(IParameterSymbol parameterSymbol)
        {
            var attributes = AttributeInstantiator.Instantiate(parameterSymbol).ToList();

            var model = new ParameterModel(parameterSymbol)
            {
                QueryAttribute = Get<QueryAttribute>(),
                IsCancellationToken = parameterSymbol.Type.ToDisplayString(SymbolDisplayFormats.TypeLookup) == "System.Threading.CancellationToken",
            };

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var attribute = (T?)attributes.FirstOrDefault(x => x is T);
                return attribute == null ? null : AttributeModel.Create(attribute);
            }
        }
    }
}