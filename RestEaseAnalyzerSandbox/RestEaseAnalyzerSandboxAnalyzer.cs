using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RestEaseAnalyzerSandbox
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RestEaseAnalyzerSandboxAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RestEaseAnalyzerSandbox";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(AnalyzeStartCompilation);
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        private void AnalyzeStartCompilation(CompilationStartAnalysisContext context)
        {
            var visitedAssemblies = new HashSet<IAssemblySymbol>();
            var assembliesToProcess = new HashSet<IAssemblySymbol>();

            Visit(visitedAssemblies, assembliesToProcess, context.Compilation.Assembly);
            static void Visit(HashSet<IAssemblySymbol> visited, HashSet<IAssemblySymbol> toProcess, IAssemblySymbol assembly)
            {
                bool hasRestEaseReference = false;
                // For each assembly, if it's got RestEase as a reference, then process it. Recursively visit all of its dependencies
                foreach (var module in assembly.Modules)
                {
                    foreach (var referencedAssembly in module.ReferencedAssemblySymbols)
                    {
                        if (referencedAssembly.Name == "RestEase")
                        {
                            hasRestEaseReference = true;
                        }
                        else if (visited.Add(referencedAssembly))
                        {
                            Visit(visited, toProcess, referencedAssembly);
                        }
                    }
                }

                if (hasRestEaseReference)
                {
                    toProcess.Add(assembly);
                }
            }

            foreach (var assemblyToProcess in assembliesToProcess)
            {
                this.ProcessAssembly(assemblyToProcess);
            }
        }

        private void ProcessAssembly(IAssemblySymbol assembly)
        {
            foreach (var namespaceOrTypeSymbol in assembly.GlobalNamespace.GetMembers())
            {
                if (namespaceOrTypeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeKind == TypeKind.Interface)
                {
                    if (this.ShouldProcessType(namedTypeSymbol))
                    {
                        this.ProcessType(namedTypeSymbol);
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
                    var baseType = attributeData.AttributeClass.BaseType;
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

        }

        private void AnalyzeCompilation(CompilationAnalysisContext context)
        {

        }
    }
}


// Visitor
public class GetAllSymbolsVisitor : SymbolVisitor
{
    public static readonly GetAllSymbolsVisitor Instance = new GetAllSymbolsVisitor();
    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        // Do what you need to here (add to collection, etc.)
    }

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        base.VisitAssembly(symbol);
    }
}