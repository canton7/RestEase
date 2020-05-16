using System;
using RestEase.Implementation.Analysis;

namespace RestEase.Implementation.Emission
{
    internal class TypeEmitter
    {
        private readonly TypeModel typeModel;

        public TypeEmitter(TypeModel typeModel)
        {
            this.typeModel = typeModel;

            this.AddInstanceCtor();
            this.AddStaticCtor();
        }

        private void AddInstanceCtor()
        {
            throw new NotImplementedException();
        }

        private void AddStaticCtor()
        {
            throw new NotImplementedException();
        }

        public EmittedProperty EmitProperty(PropertyModel property)
        {
            throw new NotImplementedException();
        }

        public void EmitRequesterProperty(PropertyModel property)
        {
            throw new NotImplementedException();
        }

        public void EmitDisposeMethod(MethodModel methodModel)
        {
            throw new NotImplementedException();
        }

        public MethodEmitter EmitMethod(MethodModel method)
        {
            throw new NotImplementedException();
        }

        public EmittedType Generate()
        {
            throw new NotImplementedException();
        }
    }
}