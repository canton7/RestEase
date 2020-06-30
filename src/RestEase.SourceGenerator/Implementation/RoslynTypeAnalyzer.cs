using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using RestEase.Implementation.Analysis;
using RestEase.Implementation.Emission;

namespace RestEase.SourceGenerator.Implementation
{
    internal class RoslynTypeAnalyzer
    {
        private readonly Compilation compilation;
        private readonly INamedTypeSymbol namedTypeSymbol;
        private readonly WellKnownSymbols wellKnownSymbols;
        private readonly AttributeInstantiator attributeInstantiator;
        private readonly DiagnosticReporter diagnosticReporter;

        public RoslynTypeAnalyzer(
            Compilation compilation,
            INamedTypeSymbol namedTypeSymbol,
            WellKnownSymbols wellKnownSymbols,
            AttributeInstantiator attributeInstantiator,
            DiagnosticReporter diagnosticReporter)
        {
            this.compilation = compilation;
            this.namedTypeSymbol = namedTypeSymbol;
            this.wellKnownSymbols = wellKnownSymbols;
            this.attributeInstantiator = attributeInstantiator;
            this.diagnosticReporter = diagnosticReporter;
        }

        public TypeModel Analyze()
        {
            var attributes = this.attributeInstantiator.Instantiate(this.namedTypeSymbol, this.diagnosticReporter).ToList();
            var typeAndInterfaceAttributes = attributes.Concat(
                this.namedTypeSymbol.AllInterfaces.SelectMany(x => this.attributeInstantiator.Instantiate(x, this.diagnosticReporter))).ToList();

            var typeModel = new TypeModel(this.namedTypeSymbol)
            {
                SerializationMethodsAttribute = Get<SerializationMethodsAttribute>(),
                BasePathAttribute = Get<BasePathAttribute>(),
                IsAccessible = this.compilation.IsSymbolAccessibleWithin(this.namedTypeSymbol, this.compilation.Assembly)
            };
            typeModel.HeaderAttributes.AddRange(GetAll<HeaderAttribute>());
            typeModel.AllowAnyStatusCodeAttributes.AddRange(GetAll<AllowAnyStatusCodeAttribute>());

            foreach (var member in this.InterfaceAndParents().SelectMany(x => x.GetMembers()))
            {
                switch (member)
                {
                    case IPropertySymbol property:
                        typeModel.Properties.Add(this.GetProperty(property));
                        break;
                    case IMethodSymbol method when method.MethodKind == MethodKind.Ordinary:
                        typeModel.Methods.Add(this.GetMethod(method));
                        break;
                    case IEventSymbol evt:
                        typeModel.Events.Add(new EventModel(evt));
                        break;
                }
            }

            return typeModel;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var (attribute, attributeData, type) = attributes.FirstOrDefault(x => x.attribute is T);
                return attribute == null ? null : AttributeModel.Create((T)attribute, attributeData, type);
            }
            IEnumerable<AttributeModel<T>> GetAll<T>() where T : Attribute =>
                typeAndInterfaceAttributes.Where(x => x.attribute is T)
                    .Select(x => AttributeModel.Create((T)x.attribute!, x.attributeData, x.declaringSymbol));
        }

        private PropertyModel GetProperty(IPropertySymbol propertySymbol)
        {
            var attributes = this.attributeInstantiator.Instantiate(propertySymbol, this.diagnosticReporter).ToList();

            var model = new PropertyModel(propertySymbol)
            {
                HeaderAttribute = Get<HeaderAttribute>(),
                PathAttribute = Get<PathAttribute>(),
                QueryAttribute = Get<QueryAttribute>(),
                HttpRequestMessagePropertyAttribute = Get<HttpRequestMessagePropertyAttribute>(),
                IsRequester = SymbolEqualityComparer.Default.Equals(propertySymbol.Type, this.wellKnownSymbols.IRequester),
                HasGetter = propertySymbol.GetMethod != null,
                HasSetter = propertySymbol.SetMethod != null,
            };

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var (attribute, attributeData, type) = attributes.FirstOrDefault(x => x.attribute is T);
                return attribute == null ? null : AttributeModel.Create((T)attribute, attributeData, type);
            }
        }

        private MethodModel GetMethod(IMethodSymbol methodSymbol)
        {
            var attributes = this.attributeInstantiator.Instantiate(methodSymbol, this.diagnosticReporter).ToList();

            var model = new MethodModel(methodSymbol)
            {
                RequestAttribute = Get<RequestAttributeBase>(),
                AllowAnyStatusCodeAttribute = Get<AllowAnyStatusCodeAttribute>(),
                SerializationMethodsAttribute = Get<SerializationMethodsAttribute>(),
                IsDisposeMethod = SymbolEqualityComparer.Default.Equals(methodSymbol, this.wellKnownSymbols.IDisposable_Dispose),
            };
            model.HeaderAttributes.AddRange(attributes.Where(x => x.attribute is HeaderAttribute)
                .Select(x => AttributeModel.Create((HeaderAttribute)x.attribute, x.attributeData, methodSymbol)));

            model.Parameters.AddRange(methodSymbol.Parameters.Select(this.GetParameter));

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var (attribute, attributeData, type) = attributes.FirstOrDefault(x => x.attribute is T);
                return attribute == null ? null : AttributeModel.Create((T)attribute, attributeData, type);
            }
        }

        private ParameterModel GetParameter(IParameterSymbol parameterSymbol)
        {
            var attributes = this.attributeInstantiator.Instantiate(parameterSymbol, this.diagnosticReporter).ToList();

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
                IsByRef = parameterSymbol.RefKind != RefKind.None,
            };

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var (attribute, attributeData, type) = attributes.FirstOrDefault(x => x.attribute is T);
                return attribute == null ? null : AttributeModel.Create((T)attribute, attributeData, type);
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