using System;
using System.CodeDom.Compiler;
using System.IO;
using RestEase.Implementation.Analysis;
using RestEase.SourceGenerator.Implementation;

namespace RestEase.Implementation.Emission
{
    internal class Emitter
    {
        private readonly WellKnownSymbols wellKnownSymbols;
        private int numTypes;

        public Emitter(WellKnownSymbols wellKnownSymbols)
        {
            this.wellKnownSymbols = wellKnownSymbols;
        }

        public TypeEmitter EmitType(TypeModel type)
        {
            this.numTypes++;
            return new TypeEmitter(type, this.wellKnownSymbols, this.numTypes);
        }
    }
}