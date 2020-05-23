using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RestEase.SourceGenerator.Implementation
{
    internal class AttributeInstantiator
    {
        private readonly WellKnownSymbols wellKnownSymbols;

        public AttributeInstantiator(WellKnownSymbols wellKnownSymbols)
        {
            this.wellKnownSymbols = wellKnownSymbols;
        }

        public IEnumerable<(Attribute? attribute, AttributeData attributeData)> Instantiate(ISymbol symbol)
        {
            return symbol.GetAttributes().Select(x => (this.Instantiate(x), x));
        }

        public Attribute? Instantiate(AttributeData attributeData)
        {
            var attributeClass = attributeData.AttributeClass;
            if (attributeClass == null)
            {
                return null;
            }

            if (SymbolEqualityComparer.Default.Equals(attributeClass, this.wellKnownSymbols.AllowAnyStatusCodeAttribute))
            {
                return this.ParseAllowAnyStatusCodeAttribute(attributeData);
            }
            if (SymbolEqualityComparer.Default.Equals(attributeClass, this.wellKnownSymbols.BasePathAttribute))
            {
                return this.ParseBasePathAttribute(attributeData);
            }
            if (SymbolEqualityComparer.Default.Equals(attributeClass, this.wellKnownSymbols.PathAttribute))
            {
                return this.ParsePathAttribute(attributeData);
            }
            if (SymbolEqualityComparer.Default.Equals(attributeClass, this.wellKnownSymbols.QueryAttribute))
            {
                return this.ParseQueryAttribute(attributeData);
            }    
            if (SymbolEqualityComparer.Default.Equals(attributeClass, this.wellKnownSymbols.SerializationMethodsAttribute))
            {
                return this.ParseSerializationMethodsAttribute(attributeData);
            }
            if (SymbolEqualityComparer.Default.Equals(attributeClass, this.wellKnownSymbols.RawQueryStringAttribute))
            {
                return this.ParseRawQueryStringAttribute(attributeData);
            }
            if (SymbolEqualityComparer.Default.Equals(attributeClass, this.wellKnownSymbols.GetAttribute))
            {
                return this.ParseRequestAttributeSubclass(attributeData, () => new GetAttribute(), x => new GetAttribute(x));
            }

            return null;
        }

        private Attribute? ParseAllowAnyStatusCodeAttribute(AttributeData attributeData)
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
                    if (namedArgument.Key == "AllowAnyStatusCode" &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_Boolean)
                    {
                        attribute.AllowAnyStatusCode = (bool)namedArgument.Value.Value!;
                    }
                }
            }

            return attribute;
        }

        private Attribute? ParseBasePathAttribute(AttributeData attributeData)
        {
            return attributeData.ConstructorArguments.Length == 1 &&
                attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String
                ? new BasePathAttribute((string)attributeData.ConstructorArguments[0].Value!)
                : null;
        }

        private Attribute? ParsePathAttribute(AttributeData attributeData)
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
                    if (namedArgument.Key == "Name" &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Name = (string?)namedArgument.Value.Value;
                    }
                    else if (namedArgument.Key == "SerializationMethod" &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.PathSerializationMethod))
                    {
                        attribute.SerializationMethod = (PathSerializationMethod)namedArgument.Value.Value!;
                    }
                    else if (namedArgument.Key == "Format" &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Format = (string?)namedArgument.Value.Value;
                    }
                    else if (namedArgument.Key == "UrlEncode" && 
                        namedArgument.Value.Type.SpecialType == SpecialType.System_Boolean)
                    {
                        attribute.UrlEncode = (bool)namedArgument.Value.Value!;
                    }
                }
            }

            return attribute;
        }

        private Attribute? ParseQueryAttribute(AttributeData attributeData)
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
                    if (namedArgument.Key == "Name" &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Name = (string?)namedArgument.Value.Value;
                    }
                    else if (namedArgument.Key == "SerializationMethod" &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.QuerySerializationMethod))
                    {
                        attribute.SerializationMethod = (QuerySerializationMethod)namedArgument.Value.Value!;
                    }
                    else if (namedArgument.Key == "Format" &&
                        namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                    {
                        attribute.Format = (string?)namedArgument.Value.Value;
                    }
                }
            }

            return attribute;
        }

        private Attribute? ParseSerializationMethodsAttribute(AttributeData attributeData)
        {
            var attribute = attributeData.ConstructorArguments.Length == 0 ? new SerializationMethodsAttribute() : null;

            if (attribute != null)
            {
                foreach (var namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == "Body" &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.BodySerializationMethod))
                    {
                        attribute.Body = (BodySerializationMethod)namedArgument.Value.Value!;
                    }
                    else if (namedArgument.Key == "Query" &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.QuerySerializationMethod))
                    {
                        attribute.Query = (QuerySerializationMethod)namedArgument.Value.Value!;
                    }
                    else if (namedArgument.Key == "Path" &&
                        SymbolEqualityComparer.Default.Equals(namedArgument.Value.Type, this.wellKnownSymbols.PathSerializationMethod))
                    {
                        attribute.Path = (PathSerializationMethod)namedArgument.Value.Value!;
                    }
                }
            }

            return attribute;
        }

        private Attribute? ParseRawQueryStringAttribute(AttributeData attributeData)
        {
            return attributeData.ConstructorArguments.Length == 0
                ? new RawQueryStringAttribute()
                : null;
        }

        private Attribute? ParseRequestAttributeSubclass(
            AttributeData attributeData,
            Func<RequestAttribute> parameterlessCtor,
            Func<string, RequestAttribute> pathCtor)
        {
            RequestAttribute attribute;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = parameterlessCtor();
            }
            else if (attributeData.ConstructorArguments.Length == 1 &&
                attributeData.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String)
            {
                attribute = pathCtor((string)attributeData.ConstructorArguments[0].Value!);
            }
            else
            {
                return null;
            }

            foreach (var namedArgument in attributeData.NamedArguments)
            {
                if (namedArgument.Key == "Path" &&
                    namedArgument.Value.Type.SpecialType == SpecialType.System_String)
                {
                    attribute.Path = (string?)namedArgument.Value.Value;
                }
            }

            return attribute;
        }
    }
}
