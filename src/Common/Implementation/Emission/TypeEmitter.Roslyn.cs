using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RestEase.Implementation.Analysis;
using RestEase.SourceGenerator.Implementation;
using static RestEase.SourceGenerator.Implementation.RoslynEmitUtils
    ;

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
        private readonly string typeNamePrefix;
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
            this.typeNamePrefix = "Implementation_" + this.index + "_";
            string constructorName = this.typeNamePrefix + this.typeModel.NamedTypeSymbol.ToDisplayString(SymbolDisplayFormats.ConstructorName);
            this.qualifiedTypeName = "global::" + this.namespaceName + "." + this.typeNamePrefix +
                this.typeModel.NamedTypeSymbol.ToDisplayString(SymbolDisplayFormats.ClassNameForReference);
            this.requesterFieldName = this.GenerateFieldName("requester");
            if (this.typeModel.HeaderAttributes.Count > 0)
            {
                this.classHeadersFieldName = this.GenerateFieldName("classHeaders");
            }

            this.AddClassDeclaration();
            this.AddInstanceCtor(constructorName);
            this.AddStaticCtor(constructorName);
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
            string typeofInterfaceName = GetGenericTypeDefinitionTypeof(this.typeModel.NamedTypeSymbol);
            string interfaceName = this.typeModel.NamedTypeSymbol.ToDisplayString(SymbolDisplayFormats.ImplementedInterface);
            string typeofName = this.typeNamePrefix + GetGenericTypeDefinitionTypeof(this.typeModel.NamedTypeSymbol, SymbolDisplayTypeQualificationStyle.NameOnly);

            this.writer.WriteLine("[assembly: global::RestEase.Implementation.RestEaseInterfaceImplementationAttribute(" +
                "typeof(" + typeofInterfaceName + "), typeof(global::" + this.namespaceName + "." + typeofName + "))]");

            this.writer.WriteLine("namespace " + this.namespaceName);
            this.writer.WriteLine("{");
            this.writer.Indent++;

            // We want class C<T> : I<T> where T : ...
            // However, ToDisplayString can only get us class C<T> where T : ...
            // Therefore, string manipulation. Also, we need to get any generic constraints with full namespace
            // (e.g. IEquatable<T>), but we don't want these for the type name.

            this.writer.Write("internal class " + this.typeNamePrefix +
                this.typeModel.NamedTypeSymbol.ToDisplayString(SymbolDisplayFormats.ClassNameForDeclaration));
            this.writer.Write(" : ");
            this.writer.Write(interfaceName);

            string classDeclarationWithConstraints = this.typeModel.NamedTypeSymbol.ToDisplayString(SymbolDisplayFormats.QualifiedClassNameWithTypeConstraints);
            int wherePosition = classDeclarationWithConstraints.IndexOf(" where");
            if (wherePosition >= 0)
            {
                this.writer.Write(classDeclarationWithConstraints.Substring(wherePosition));
            }
            this.writer.WriteLine();

            this.writer.WriteLine("{");
            this.writer.Indent++;

            if (this.classHeadersFieldName != null)
            {
                this.writer.WriteLine("private static readonly global::System.Collections.Generic.KeyValuePair<string, string>[] " +
                    this.classHeadersFieldName + ";");
            }
            this.writer.WriteLine("private readonly global::RestEase.IRequester " + this.requesterFieldName + ";");
        }

        private void AddInstanceCtor(string constructorName)
        {
            this.writer.WriteLine("public " + constructorName + "(global::RestEase.IRequester " + this.requesterFieldName + ")");
            ;
            this.writer.WriteLine("{");
            this.writer.Indent++;

            this.writer.WriteLine("this." + this.requesterFieldName + " = requester;");

            this.writer.Indent--;
            this.writer.WriteLine("}");
        }

        private void AddStaticCtor(string constructorName)
        {
            if (this.classHeadersFieldName == null)
                return;

            this.writer.WriteLine("static " + constructorName + "()");
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

            var sourceText = SourceText.From(this.stringWriter.ToString(), Encoding.UTF8);
            return new EmittedType(sourceText);
        }
    }
}