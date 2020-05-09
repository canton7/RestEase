using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RestEase.Implementation.Analysis;

namespace RestEase.Implementation.Emission
{
    internal class TypeEmitter
    {
        private readonly TypeBuilder typeBuilder;
        private readonly TypeModel typeModel;
        private readonly FieldBuilder requesterField;
        private readonly FieldBuilder? classHeadersField;

        private int numMethods;

        public TypeEmitter(TypeBuilder typeBuilder, TypeModel typeModel)
        {
            this.typeBuilder = typeBuilder;
            this.typeModel = typeModel;

            // Define a readonly field which holds a reference to the IRequester
            this.requesterField = typeBuilder.DefineField("requester", typeof(IRequester), FieldAttributes.Private | FieldAttributes.InitOnly);

            if (typeModel.HeaderAttributes.Count > 0)
            {
                this.classHeadersField = typeBuilder.DefineField("classHeaders", typeof(KeyValuePair<string, string>[]), FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);
            }

            this.AddInstanceCtor();
            this.AddStaticCtor();
        }

        private void AddInstanceCtor()
        {
            // Add a constructor which takes the IRequester and assigns it to the field
            // public Name(IRequester requester)
            // {
            //     this.requester = requester;
            // }
            var ctorBuilder = this.typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(IRequester) });
            var ilGenerator = ctorBuilder.GetILGenerator();
            // Load 'this' and the requester onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            // Store the requester into this.requester
            ilGenerator.Emit(OpCodes.Stfld, this.requesterField);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private void AddStaticCtor()
        {
            if (this.classHeadersField == null)
                return;

            var headers = this.typeModel.HeaderAttributes;
            var staticCtorBuilder = this.typeBuilder.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, new Type[0]);
            var ilGenerator = staticCtorBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldc_I4, headers.Count);
            ilGenerator.Emit(OpCodes.Newarr, typeof(KeyValuePair<string, string>));
            for (int i = 0; i < headers.Count; i++)
            {
                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.Emit(OpCodes.Ldc_I4, i);
                ilGenerator.Emit(OpCodes.Ldstr, headers[i].Attribute.Name);
                // We already check that it's got a non-null value
                ilGenerator.Emit(OpCodes.Ldstr, headers[i].Attribute.Value);
                ilGenerator.Emit(OpCodes.Newobj, MethodInfos.KeyValuePair_Ctor_String_String);
                ilGenerator.Emit(OpCodes.Stelem, typeof(KeyValuePair<string, string>));
            }
            ilGenerator.Emit(OpCodes.Stsfld, this.classHeadersField);

            ilGenerator.Emit(OpCodes.Ret);
        }

        public EmittedProperty EmitProperty(PropertyModel property)
        {
            MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName;

            var propertyBuilder = this.typeBuilder.DefineProperty(property.PropertyInfo.Name, PropertyAttributes.None, property.PropertyInfo.PropertyType, null);
            var getter = this.typeBuilder.DefineMethod(property.PropertyInfo.GetMethod.Name, attributes, property.PropertyInfo.PropertyType, new Type[0]);
            var setter = this.typeBuilder.DefineMethod(property.PropertyInfo.SetMethod.Name, attributes, null, new Type[] { property.PropertyInfo.PropertyType });
            var backingField = this.typeBuilder.DefineField("bk_" + property.PropertyInfo.Name, property.PropertyInfo.PropertyType, FieldAttributes.Private);

            var getterIlGenerator = getter.GetILGenerator();
            getterIlGenerator.Emit(OpCodes.Ldarg_0);
            getterIlGenerator.Emit(OpCodes.Ldfld, backingField);
            getterIlGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getter);

            var setterIlGenerator = setter.GetILGenerator();
            setterIlGenerator.Emit(OpCodes.Ldarg_0);
            setterIlGenerator.Emit(OpCodes.Ldarg_1);
            setterIlGenerator.Emit(OpCodes.Stfld, backingField);
            setterIlGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setter);

            return new EmittedProperty(property, backingField);
        }

        public void EmitRequesterProperty(PropertyModel property)
        {
            MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
            
            var propertyBuilder = this.typeBuilder.DefineProperty(property.PropertyInfo.Name, PropertyAttributes.None, property.PropertyInfo.PropertyType, null);
            var getter = this.typeBuilder.DefineMethod(property.PropertyInfo.GetMethod.Name, attributes, property.PropertyInfo.PropertyType, new Type[0]);
            var getterIlGenerator = getter.GetILGenerator();
            getterIlGenerator.Emit(OpCodes.Ldarg_0);
            getterIlGenerator.Emit(OpCodes.Ldfld, this.requesterField);
            getterIlGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getter);
        }

        public void EmitDisposeMethod(MethodModel methodModel)
        {
            var methodBuilder = this.typeBuilder.DefineMethod(
                methodModel.MethodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual,
                methodModel.MethodInfo.ReturnType,
                methodModel.Parameters.Select(x => x.ParameterInfo.ParameterType).ToArray());
            this.typeBuilder.DefineMethodOverride(methodBuilder, methodModel.MethodInfo);
            var ilGenerator = methodBuilder.GetILGenerator();
            
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, this.requesterField);
            ilGenerator.Emit(OpCodes.Callvirt, MethodInfos.IDisposable_Dispose);
            ilGenerator.Emit(OpCodes.Ret);
        }

        public MethodEmitter EmitMethod(MethodModel method)
        { 
            var methodEmitter = new MethodEmitter(this.typeBuilder, method, this.numMethods, this.requesterField, this.classHeadersField);
            this.numMethods++;
            return methodEmitter;
        }

        public EmittedType Generate()
        {
            Type constructedType;
            try
            {
                constructedType = this.typeBuilder.CreateTypeInfo().AsType();
            }
            catch (TypeLoadException e)
            {
                string msg = string.Format("Unable to create implementation for interface {0}. Ensure that the interface is public, or add [assembly: InternalsVisibleTo(RestClient.FactoryAssemblyName)] to your AssemblyInfo.cs", this.typeModel.Type.FullName);
                throw new ImplementationCreationException(msg, e);
            }

            return new EmittedType(constructedType);
        }
    }
}