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

        private MethodModel GetMethod(IMethodSymbol method)
        {
            var attributes = AttributeInstantiator.Instantiate(method).ToList();

            var model = new MethodModel(method)
            {
                RequestAttribute = Get<RequestAttribute>(),
            };

            model.Parameters.AddRange(method.Parameters.Select(this.GetParameter));

            return model;

            AttributeModel<T>? Get<T>() where T : Attribute
            {
                var attribute = (T?)attributes.FirstOrDefault(x => x is T);
                return attribute == null ? null : AttributeModel.Create(attribute);
            }
        }

        private ParameterModel GetParameter(IParameterSymbol parameterSymbol)
        {
            var symbolDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

            var model = new ParameterModel(parameterSymbol)
            {
                IsCancellationToken = parameterSymbol.Type.ToDisplayString(symbolDisplayFormat) == "System.Threading.CancellationToken",
            };

            return model;
        }
    }
}