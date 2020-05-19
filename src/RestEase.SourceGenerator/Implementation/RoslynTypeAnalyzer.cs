using System;
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
            var typeModel = new TypeModel(this.namedTypeSymbol);

            foreach (var member in this.namedTypeSymbol.GetMembers())
            {
                switch (member)
                {
                    case IMethodSymbol method:
                        typeModel.Methods.Add(this.GetMethod(method));
                        break;
                }
            }

            return typeModel;
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