using System;
using System.CodeDom.Compiler;
using System.IO;
using RestEase.Implementation.Analysis;

namespace RestEase.Implementation.Emission
{
    internal class Emitter
    {
        private int numTypes;

        public Emitter()
        {
        }

        public TypeEmitter EmitType(TypeModel type)
        {
            this.numTypes++;
            return new TypeEmitter(type, this.numTypes);
        }
    }
}