using Microsoft.CodeAnalysis;
using RestEase.Implementation.Analysis;
using RestEase.SourceGenerator.Implementation;

namespace RestEase.Implementation.Emission
{
    internal class Emitter
    {
        private readonly Compilation compilation;
        private readonly WellKnownSymbols wellKnownSymbols;
        private int numTypes;

        public Emitter(Compilation compilation, WellKnownSymbols wellKnownSymbols)
        {
            this.compilation = compilation;
            this.wellKnownSymbols = wellKnownSymbols;
        }

        public TypeEmitter EmitType(TypeModel type)
        {
            this.numTypes++;
            return new TypeEmitter(type, this.compilation, this.wellKnownSymbols, this.numTypes);
        }
    }
}