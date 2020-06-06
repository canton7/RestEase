using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RestEase.Implementation.Emission;

namespace RestEase.SourceGenerator.Implementation
{
    internal class AttributeInstantiator
    {
        private delegate Attribute? Parser(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter);
        private readonly WellKnownSymbols wellKnownSymbols;
        private readonly Dictionary<INamedTypeSymbol, Parser> lookup;

        public AttributeInstantiator(WellKnownSymbols wellKnownSymbols)
        {
            this.wellKnownSymbols = wellKnownSymbols;

            this.lookup = new Dictionary<INamedTypeSymbol, Parser>(19, SymbolEqualityComparer.Default);
            Add(this.wellKnownSymbols.AllowAnyStatusCodeAttribute, this.ParseAllowAnyStatusCodeAttribute);
            Add(this.wellKnownSymbols.BasePathAttribute, this.ParseBasePathAttribute);
            Add(this.wellKnownSymbols.PathAttribute, this.ParsePathAttribute);
            Add(this.wellKnownSymbols.QueryAttribute, this.ParseQueryAttribute);
            Add(this.wellKnownSymbols.SerializationMethodsAttribute, this.ParseSerializationMethodsAttribute);
            Add(this.wellKnownSymbols.RawQueryStringAttribute, this.ParseRawQueryStringAttribute);
            Add(this.wellKnownSymbols.BodyAttribute, this.ParseBodyAttribute);
            Add(this.wellKnownSymbols.HeaderAttribute, this.ParseHeaderAttribute);
            Add(this.wellKnownSymbols.HttpRequestMessagePropertyAttribute, this.ParseHttpRequestMessagePropertyAttribute);
            Add(this.wellKnownSymbols.QueryMapAttribute, this.ParseQueryMapAttribute);
            Add(this.wellKnownSymbols.RequestAttribute, this.ParseRequestAttribute);
            Add(this.wellKnownSymbols.DeleteAttribute, (x, s, d) => this.ParseRequestAttributeSubclass(x, s, d, () => new DeleteAttribute(), s => new DeleteAttribute(s)));
            Add(this.wellKnownSymbols.GetAttribute, (x, s, d) => this.ParseRequestAttributeSubclass(x, s, d, () => new GetAttribute(), s => new GetAttribute(s)));
            Add(this.wellKnownSymbols.HeadAttribute, (x, s ,d) => this.ParseRequestAttributeSubclass(x, s, d, () => new HeadAttribute(), s => new HeadAttribute(s)));
            Add(this.wellKnownSymbols.OptionsAttribute, (x, s, d) => this.ParseRequestAttributeSubclass(x, s, d, () => new OptionsAttribute(), s => new OptionsAttribute(s)));
            Add(this.wellKnownSymbols.PostAttribute, (x, s, d) => this.ParseRequestAttributeSubclass(x, s, d, () => new PostAttribute(), s => new PostAttribute(s)));
            Add(this.wellKnownSymbols.PutAttribute, (x, s, d) => this.ParseRequestAttributeSubclass(x, s, d, () => new PutAttribute(), s => new PutAttribute(s)));
            Add(this.wellKnownSymbols.TraceAttribute, (x, s, d) => this.ParseRequestAttributeSubclass(x, s, d, () => new TraceAttribute(), s => new TraceAttribute(s)));
            Add(this.wellKnownSymbols.PatchAttribute, (x, s, d) => this.ParseRequestAttributeSubclass(x, s, d, () => new PatchAttribute(), s => new PatchAttribute(s)));


            void Add(INamedTypeSymbol? symbol, Parser func)
            {
                if (symbol != null)
                {
                    this.lookup[symbol] = func;
                }
            }
        }

        public bool IsRestEaseAttribute(INamedTypeSymbol symbol)
        {
            return this.lookup.ContainsKey(symbol);
        }

        public IEnumerable<(Attribute attribute, AttributeData attributeData, ISymbol declaringSymbol)> Instantiate(ISymbol symbol, DiagnosticReporter diagnosticReporter)
        {
            return symbol.GetAttributes()
                .Select(x => (this.Instantiate(x, symbol, diagnosticReporter), x, symbol))
                .Where(x => x.Item1 != null)!;
        }

        public Attribute? Instantiate(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            var attributeClass = attributeData.AttributeClass;
            if (attributeClass == null)
            {
                return null;
            }

            if (this.lookup.TryGetValue(attributeClass, out var parser))
            {
                return parser(attributeData, declaringSymbol, diagnosticReporter);
            }

            return null;
        }

        private Attribute? ParseAllowAnyStatusCodeAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            AllowAnyStatusCodeAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = new AllowAnyStatusCodeAttribute();
            }
            else if (attributeData.ConstructorArguments.Length == 1 &&
                attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_Boolean)
            {
                attribute = new AllowAnyStatusCodeAttribute((bool)attributeData.ConstructorArguments[0].Value!);
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == nameof(AllowAnyStatusCodeAttribute.AllowAnyStatusCode) &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_Boolean)
                    {
                        attribute.AllowAnyStatusCode = (bool)namedArgument.Value.Value!;
                    }
                    else
                    {
                        diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                    }
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParseBasePathAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            BasePathAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 1 &&
                attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String)
            {
                attribute = new BasePathAttribute((string)attributeData.ConstructorArguments[0].Value!);
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParsePathAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            PathAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = new PathAttribute();
            }
            else if (attributeData.ConstructorArguments.Length == 1)
            {
                if (attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String)
                {
                    attribute = new PathAttribute((string)attributeData.ConstructorArguments[0].Value!);
                }
                else if (SymbolEqualityComparer.Default.Equals(attributeData.ConstructorArguments[0].Type, this.wellKnownSymbols.PathSerializationMethod))
                {
                    attribute = new PathAttribute((PathSerializationMethod)attributeData.ConstructorArguments[0].Value!);
                }
            }
            else if (attributeData.ConstructorArguments.Length == 2)
            {
                if (attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String &&
                    SymbolEqualityComparer.Default.Equals(attributeData.ConstructorArguments[1].Type, this.wellKnownSymbols.PathSerializationMethod))
                {
                    attribute = new PathAttribute((string)attributeData.ConstructorArguments[0].Value!, (PathSerializationMethod)attributeData.ConstructorArguments[1].Value!);
                }
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == nameof(PathAttribute.Name) &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Name = (string?)namedArgument.Value.Value;
                    }
                    else if (namedArgument.Key == nameof(PathAttribute.SerializationMethod) &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.PathSerializationMethod))
                    {
                        attribute.SerializationMethod = (PathSerializationMethod)namedArgument.Value.Value!;
                    }
                    else if (namedArgument.Key == nameof(PathAttribute.Format) &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Format = (string?)namedArgument.Value.Value;
                    }
                    else if (namedArgument.Key == nameof(PathAttribute.UrlEncode) &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_Boolean)
                    {
                        attribute.UrlEncode = (bool)namedArgument.Value.Value!;
                    }
                    else
                    {
                        diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                    }
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParseQueryAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            QueryAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = new QueryAttribute();
            }
            else if (attributeData.ConstructorArguments.Length == 1)
            {
                if (attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String)
                {
                    attribute = new QueryAttribute((string?)attributeData.ConstructorArguments[0].Value);
                }
                else if (SymbolEqualityComparer.Default.Equals(attributeData.ConstructorArguments[0].Type, this.wellKnownSymbols.QuerySerializationMethod))
                {
                    attribute = new QueryAttribute((QuerySerializationMethod)attributeData.ConstructorArguments[0].Value!);
                }
            }
            else if (attributeData.ConstructorArguments.Length == 2)
            {
                if (attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String &&
                    SymbolEqualityComparer.Default.Equals(attributeData.ConstructorArguments[1].Type, this.wellKnownSymbols.QuerySerializationMethod))
                {
                    attribute = new QueryAttribute((string?)attributeData.ConstructorArguments[0].Value, (QuerySerializationMethod)attributeData.ConstructorArguments[1].Value!);
                }
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == nameof(QueryAttribute.Name) &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Name = (string?)namedArgument.Value.Value;
                    }
                    else if (namedArgument.Key == nameof(QueryAttribute.SerializationMethod) &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.QuerySerializationMethod))
                    {
                        attribute.SerializationMethod = (QuerySerializationMethod)namedArgument.Value.Value!;
                    }
                    else if (namedArgument.Key == nameof(QueryAttribute.Format) &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Format = (string?)namedArgument.Value.Value;
                    }
                    else
                    {
                        diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                    }
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParseSerializationMethodsAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            SerializationMethodsAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = new SerializationMethodsAttribute();
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == nameof(SerializationMethodsAttribute.Body) &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.BodySerializationMethod))
                    {
                        attribute.Body = (BodySerializationMethod)namedArgument.Value.Value!;
                    }
                    else if (namedArgument.Key == nameof(SerializationMethodsAttribute.Query) &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.QuerySerializationMethod))
                    {
                        attribute.Query = (QuerySerializationMethod)namedArgument.Value.Value!;
                    }
                    else if (namedArgument.Key == nameof(SerializationMethodsAttribute.Path) &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.PathSerializationMethod))
                    {
                        attribute.Path = (PathSerializationMethod)namedArgument.Value.Value!;
                    }
                    else
                    {
                        diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                    }
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParseRawQueryStringAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            RawQueryStringAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = new RawQueryStringAttribute();
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParseBodyAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            BodyAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = new BodyAttribute();
            }
            else if (attributeData.ConstructorArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(attributeData.ConstructorArguments[0].Type, this.wellKnownSymbols.BodySerializationMethod))
            {
                attribute = new BodyAttribute((BodySerializationMethod)attributeData.ConstructorArguments[0].Value!);
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParseHeaderAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            HeaderAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 1 &&
                attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String)
            {
                attribute = new HeaderAttribute((string)attributeData.ConstructorArguments[0].Value!);
            }
            else if (attributeData.ConstructorArguments.Length == 2 &&
                attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String &&
                attributeData.ConstructorArguments[1].Type.SpecialType == SpecialType.System_String)
            {
                attribute = new HeaderAttribute(
                    (string)attributeData.ConstructorArguments[0].Value!,
                    (string)attributeData.ConstructorArguments[1].Value!);
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == nameof(HeaderAttribute.Format) &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Format = (string?)namedArgument.Value.Value;
                    }
                    else
                    {
                        diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                    }
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParseHttpRequestMessagePropertyAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            HttpRequestMessagePropertyAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = new HttpRequestMessagePropertyAttribute();
            }
            else if (attributeData.ConstructorArguments.Length == 1 &&
                attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String)
            {
                attribute = new HttpRequestMessagePropertyAttribute((string)attributeData.ConstructorArguments[0].Value!);
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == nameof(HttpRequestMessagePropertyAttribute.Key) &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Key = (string?)namedArgument.Value.Value;
                    }
                    else
                    {
                        diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                    }
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParseQueryMapAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            QueryMapAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = new QueryMapAttribute();
            }
            else if (attributeData.ConstructorArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(attributeData.ConstructorArguments[0].Type, this.wellKnownSymbols.QuerySerializationMethod))
            {
                attribute = new QueryMapAttribute((QuerySerializationMethod)attributeData.ConstructorArguments[0].Value!);
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == nameof(QueryMapAttribute.SerializationMethod) &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.QueryMapAttribute))
                    {
                        attribute.SerializationMethod = (QuerySerializationMethod)namedArgument.Value.Value!;
                    }
                    else
                    {
                        diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                    }
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParseRequestAttribute(AttributeData attributeData, ISymbol declaringSymbol, DiagnosticReporter diagnosticReporter)
        {
            RequestAttribute? attribute = null;
            if (attributeData.ConstructorArguments.Length == 1 &&
                attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String)
            {
                attribute = new RequestAttribute((string)attributeData.ConstructorArguments[0].Value!);
            }
            else if (attributeData.ConstructorArguments.Length == 2 &&
                attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String &&
                attributeData.ConstructorArguments[1].Type.SpecialType == SpecialType.System_String)
            {
                attribute = new RequestAttribute(
                    (string)attributeData.ConstructorArguments[0].Value!,
                    (string)attributeData.ConstructorArguments[1].Value!);
            }

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == nameof(RequestAttributeBase.Path) &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Path = (string?)namedArgument.Value.Value;
                    }
                    else
                    {
                        diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                    }
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }

        private Attribute? ParseRequestAttributeSubclass(
            AttributeData attributeData,
            ISymbol declaringSymbol,
            DiagnosticReporter diagnosticReporter,
            Func<RequestAttributeBase> parameterlessCtor,
            Func<string, RequestAttributeBase> pathCtor)
        {
            RequestAttributeBase? attribute = null;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = parameterlessCtor();
            }
            else if (attributeData.ConstructorArguments.Length == 1 &&
                attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String)
            {
                attribute = pathCtor((string)attributeData.ConstructorArguments[0].Value!);
            }

            if (attribute != null)

            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == nameof(RequestAttributeBase.Path) &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Path = (string?)namedArgument.Value.Value;
                    }
                    else
                    {
                        diagnosticReporter.ReportAttributePropertyNotRecognised(attributeData, namedArgument, declaringSymbol);
                    }
                }
            }
            else
            {
                diagnosticReporter.ReportAttributeConstructorNotRecognised(attributeData, declaringSymbol);
            }

            return attribute;
        }
    }
}
