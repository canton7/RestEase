using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RestEase.Implementation.Analysis;
using RestEase.SourceGenerator.Implementation;

namespace RestEase.Implementation.Emission
{
    internal class TypeEmitter
    {
        private readonly StringWriter stringWriter = new StringWriter();
        private readonly IndentedTextWriter writer;
        private readonly TypeModel typeModel;
        private readonly WellKnownSymbols wellKnownSymbols;
        private readonly int index;
        private readonly string namespaceName;
        private readonly string typeName;
        private readonly string requesterFieldName;
        private int numMethods;

        public TypeEmitter(TypeModel typeModel, WellKnownSymbols wellKnownSymbols, int index)
        {
            this.typeModel = typeModel;
            this.wellKnownSymbols = wellKnownSymbols;
            this.index = index;
            this.writer = new IndentedTextWriter(this.stringWriter);
            this.namespaceName = this.typeModel.NamedTypeSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormats.Namespace) + ".RestEaseGeneratedTypes";
            this.typeName = "Implementation_" + this.index + "_" + this.typeModel.NamedTypeSymbol.ToDisplayString(SymbolDisplayFormats.ClassDeclaration);
            this.requesterFieldName = this.GenerateRequesterFieldName();

            this.AddClassDeclaration();
            this.AddInstanceCtor();
            this.AddStaticCtor();
        }

        private string GenerateRequesterFieldName()
        {
            string? name = "requester";
            if (this.typeModel.NamedTypeSymbol.GetMembers().Any(x => x.Name == name))
            {
                int i = 1;
                do
                {
                    name = "requester" + i;
                    i++;
                } while (this.typeModel.NamedTypeSymbol.GetMembers().Any(x => x.Name == name));
            }
            return name;
        }

        private void AddClassDeclaration()
        {
            string interfaceName = this.typeModel.NamedTypeSymbol.ToDisplayString(SymbolDisplayFormats.ImplementedInterface);

            this.writer.WriteLine("[assembly: global::RestEase.Implementation.RestEaseInterfaceImplementationAttribute(" +
                "typeof(" + interfaceName + "), typeof(global::" + this.namespaceName + "." + this.typeName + "))]");

            this.writer.WriteLine("namespace " + this.namespaceName);
            this.writer.WriteLine("{");
            this.writer.Indent++;

            this.writer.Write("internal class " + this.typeName);
            this.writer.Write(" : ");
            this.writer.WriteLine(interfaceName);
            this.writer.WriteLine("{");
            this.writer.Indent++;

            // TODO: Do we need a special name for this?
            this.writer.WriteLine("private readonly global::RestEase.IRequester " + this.requesterFieldName + ";");
        }

        private void AddInstanceCtor()
        {
            this.writer.WriteLine("public " + this.typeName + "(global::RestEase.IRequester " + this.requesterFieldName + ")");
            this.writer.WriteLine("{");
            this.writer.Indent++;

            this.writer.WriteLine("this." + this.requesterFieldName + " = requester;");

            this.writer.Indent--;
            this.writer.WriteLine("}");
        }

        private void AddStaticCtor()
        {
            // TODO...
        }

        public EmittedProperty EmitProperty(PropertyModel propertyModel)
        {
            // Because the property is declared on the interface, we don't get any generated accessibility
            this.writer.WriteLine("public " + propertyModel.PropertySymbol.ToDisplayString(SymbolDisplayFormats.PropertyDeclaration));
            return new EmittedProperty(propertyModel);
        }

        public void EmitRequesterProperty(PropertyModel propertyModel)
        {
            throw new NotImplementedException();
        }

        public void EmitDisposeMethod(MethodModel methodModel)
        {
            throw new NotImplementedException();
        }

        public MethodEmitter EmitMethod(MethodModel methodModel)
        {
            this.numMethods++;
            return new MethodEmitter(
                methodModel,
                this.writer,
                this.wellKnownSymbols,
                this.namespaceName + "." + this.typeName,
                this.requesterFieldName,
                this.numMethods);
        }

        public EmittedType Generate()
        {
            this.writer.Indent--;
            this.writer.WriteLine("}");
            this.writer.Indent--;
            this.writer.WriteLine("}");
            this.writer.Flush();

            var sourceText = SourceText.From(this.stringWriter.ToString());
            return new EmittedType(sourceText);
        }
    }
}