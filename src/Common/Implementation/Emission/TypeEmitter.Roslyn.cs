using System;
using System.CodeDom.Compiler;
using System.IO;
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
        private readonly int index;
        private readonly string namespaceName;
        private readonly string typeName;
        private int numMethods;

        public TypeEmitter(TypeModel typeModel, int index)
        {
            this.typeModel = typeModel;
            this.index = index;
            this.writer = new IndentedTextWriter(this.stringWriter);
            this.namespaceName = this.typeModel.NamedTypeSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormats.Namespace) + ".RestEaseGeneratedTypes";
            this.typeName = "Implementation_" + this.index + "_" + this.typeModel.NamedTypeSymbol.ToDisplayString(SymbolDisplayFormats.ClassDeclaration);

            this.AddClassDeclaration();
            this.AddInstanceCtor();
            this.AddStaticCtor();
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
            this.writer.WriteLine("private readonly global::RestEase.IRequester requester;");
        }

        private void AddInstanceCtor()
        {
            this.writer.WriteLine("public " + this.typeName + "(global::RestEase.IRequester requester)");
            this.writer.WriteLine("{");
            this.writer.Indent++;

            this.writer.WriteLine("this.requester = requester;");

            this.writer.Indent--;
            this.writer.WriteLine("}");
        }

        private void AddStaticCtor()
        {
            // TODO...
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
            this.numMethods++;
            return new MethodEmitter(method, this.writer, this.namespaceName + "." + this.typeName, this.numMethods);
        }

        public EmittedType Generate()
        {
            this.writer.WriteLine("}");
            this.writer.Indent--;
            this.writer.WriteLine("}");
            this.writer.Flush();

            var sourceText = SourceText.From(this.stringWriter.ToString());
            return new EmittedType(sourceText);
        }
    }
}