using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RestEase.Implementation.Analysis;
using RestEase.SourceGenerator.Implementation;
using static RestEase.SourceGenerator.Implementation.EmitUtils;

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
        private readonly string qualifiedTypeName;
        private readonly string requesterFieldName;
        private readonly string? classHeadersFieldName;

        private readonly List<string> generatedFieldNames = new List<string>();

        public TypeEmitter(TypeModel typeModel, WellKnownSymbols wellKnownSymbols, int index)
        {
            this.typeModel = typeModel;
            this.wellKnownSymbols = wellKnownSymbols;
            this.index = index;
            this.writer = new IndentedTextWriter(this.stringWriter);
            this.namespaceName = this.typeModel.NamedTypeSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormats.Namespace) + ".RestEaseGeneratedTypes";
            this.typeName = "Implementation_" + this.index + "_" + this.typeModel.NamedTypeSymbol.ToDisplayString(SymbolDisplayFormats.ClassDeclaration);
            this.qualifiedTypeName = "global::" + this.namespaceName + "." + this.typeName;
            this.requesterFieldName = this.GenerateFieldName("requester");
            if (this.typeModel.HeaderAttributes.Count > 0)
            {
                this.classHeadersFieldName = this.GenerateFieldName("classHeaders");
            }

            this.AddClassDeclaration();
            this.AddInstanceCtor();
            this.AddStaticCtor();
        }

        private string GenerateFieldName(string baseName)
        {
            string? name = baseName;
            if (this.generatedFieldNames.Contains(name) || this.typeModel.NamedTypeSymbol.GetMembers().Any(x => x.Name == name))
            {
                int i = 1;
                do
                {
                    name = baseName + i;
                    i++;
                } while (this.generatedFieldNames.Contains(name) || this.typeModel.NamedTypeSymbol.GetMembers().Any(x => x.Name == name));
            }
            this.generatedFieldNames.Add(name);
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

            if (this.classHeadersFieldName != null)
            {
                this.writer.WriteLine("private static readonly global::System.Collections.Generic.KeyValuePair<string, string>[] " +
                    this.classHeadersFieldName + ";");
            }
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
            if (this.classHeadersFieldName == null)
                return;

            this.writer.WriteLine("static " + this.typeName + "()");
            this.writer.WriteLine("{");
            this.writer.Indent++;

            this.writer.WriteLine(this.qualifiedTypeName + "." + this.classHeadersFieldName +
                " = new global::System.Collections.Generic.KeyValuePair<string, string>[]");
            this.writer.WriteLine("{");
            this.writer.Indent++;
            foreach (var header in this.typeModel.HeaderAttributes)
            {
                this.writer.WriteLine("new global::System.Collections.Generic.KeyValuePair<string, string>(" +
                    QuoteString(header.Attribute.Name) + ", " + QuoteString(header.Attribute.Value) + "),");
            }
            this.writer.Indent--;
            this.writer.WriteLine("};");

            this.writer.Indent--;
            this.writer.WriteLine("}");
        }

        public EmittedProperty EmitProperty(PropertyModel propertyModel)
        {
            // Because the property is declared on the interface, we don't get any generated accessibility
            this.writer.WriteLine("public " + propertyModel.PropertySymbol.ToDisplayString(SymbolDisplayFormats.PropertyDeclaration));
            return new EmittedProperty(propertyModel);
        }

        public void EmitRequesterProperty(PropertyModel propertyModel)
        {
            this.writer.WriteLine("public global::RestEase.IRequester " +
                propertyModel.PropertySymbol.ToDisplayString(SymbolDisplayFormats.SymbolName) +
                " { get { return this." + this.requesterFieldName + "; } }");
        }

        public void EmitDisposeMethod(MethodModel _)
        {
            this.writer.WriteLine("void global::System.IDisposable.Dispose()");
            this.writer.WriteLine("{");
            this.writer.Indent++;
            this.writer.WriteLine("this." + this.requesterFieldName + ".Dispose();");
            this.writer.Indent--;
            this.writer.WriteLine("}");
        }

        public MethodEmitter EmitMethod(MethodModel methodModel)
        {
            return new MethodEmitter(
                methodModel,
                this.writer,
                this.wellKnownSymbols,
                this.qualifiedTypeName,
                this.requesterFieldName,
                this.classHeadersFieldName,
                this.GenerateFieldName("methodInfo_" + methodModel.MethodSymbol.Name));
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