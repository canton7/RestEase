using System;
using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal partial class ParameterModel
    {
        public IParameterSymbol ParameterSymbol { get; }

        public string Name => this.ParameterSymbol.Name;

        public ParameterModel(IParameterSymbol parameterSymbol)
        {
            this.ParameterSymbol = parameterSymbol;
        }
    }
}