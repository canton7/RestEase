using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using RestEase.SourceGenerator.Implementation;

namespace RestEase.SourceGenerator
{
    internal class Processor : SymbolVisitor
    {
        private readonly SourceGeneratorContext context;
        private readonly RoslynImplementationFactory factory = new RoslynImplementationFactory();

        public Processor(SourceGeneratorContext context) => this.context = context;

        public void Process()
        {
            //var visitedAssemblies = new HashSet<IAssemblySymbol>();
            //var assembliesToProcess = new HashSet<IAssemblySymbol>();

            //Visit(visitedAssemblies, assembliesToProcess, this.context.Compilation.Assembly);
            //static void Visit(HashSet<IAssemblySymbol> visited, HashSet<IAssemblySymbol> toProcess, IAssemblySymbol assembly)
            //{
            //    bool hasRestEaseReference = false;
            //    // For each assembly, if it's got RestEase as a reference, then process it. Recursively visit all of its dependencies
            //    foreach (var module in assembly.Modules)
            //    {
            //        foreach (var referencedAssembly in module.ReferencedAssemblySymbols)
            //        {
            //            if (referencedAssembly.Name == "RestEase")
            //            {
            //                hasRestEaseReference = true;
            //            }
            //            else if (visited.Add(referencedAssembly))
            //            {
            //                Visit(visited, toProcess, referencedAssembly);
            //            }
            //        }
            //    }

            //    if (hasRestEaseReference)
            //    {
            //        toProcess.Add(assembly);
            //    }
            //}

            //foreach (var assemblyToProcess in assembliesToProcess)
            //{
            //    assemblyToProcess.GlobalNamespace.Accept(this);
            //}

            this.context.Compilation.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TypeKind == TypeKind.Interface)
            {
                if (this.ShouldProcessType(namedTypeSymbol))
                {
                    this.ProcessType(namedTypeSymbol);
                }
            }
            else if (namedTypeSymbol.TypeKind == TypeKind.Class)
            {
                // Handle nested types
                foreach (var member in namedTypeSymbol.GetMembers())
                {
                    if (member.Kind == SymbolKind.NamedType)
                    {
                        member.Accept(this);
                    }
                }
            }
        }

        private bool ShouldProcessType(INamedTypeSymbol namedTypeSymbol)
        {
            foreach (var memberSymbol in namedTypeSymbol.GetMembers())
            {
                foreach (var attributeData in memberSymbol.GetAttributes())
                {
                    var baseType = attributeData.AttributeClass?.BaseType;
                    if (baseType != null && baseType.ContainingNamespace.Name == "RestEase" && baseType.Name == "RequestAttribute")
                    {
                        return true;
                    }
                }

                // We expect every member to have a RequestAttribute
                return false;
            }

            return false;
        }

        private void ProcessType(INamedTypeSymbol namedTypeSymbol)
        {
            var (sourceText, diagnostics) = this.factory.CreateImplementation(namedTypeSymbol);
            foreach (var diagnostic in diagnostics)
            {
                this.context.ReportDiagnostic(diagnostic);
            }

            if (sourceText != null)
            {
                this.context.AddSource("RestEase_" + namedTypeSymbol.Name, sourceText);
            }
        }
    }
}
