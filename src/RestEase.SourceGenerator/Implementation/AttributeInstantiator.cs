using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace RestEase.SourceGenerator.Implementation
{
    public static class AttributeInstantiator
    {
        private static readonly Assembly assembly = typeof(AttributeInstantiator).Assembly;

        public static IEnumerable<Attribute?> Instantiate(IMethodSymbol method)
        {
            return method.GetAttributes().Select(Instantiate);
        }

        public static Attribute? Instantiate(AttributeData attributeData)
        {
            string? attributeMetadataName = attributeData.AttributeClass?.ToDisplayString(SymbolDisplayFormats.TypeLookup);
            if (attributeMetadataName == null)
            {
                return null;
            }

            var attribute = attributeMetadataName switch
            {
                "RestEase.GetAttribute" => ParseRequestAttributeSubclass(attributeData, () => new GetAttribute(), x => new GetAttribute(x)),
                _ => null,
            };

            return attribute;
        }

        private static Attribute? ParseRequestAttributeSubclass(
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
                if (namedArgument.Key == "Path")
                {
                    attribute.Path = (string?)namedArgument.Value.Value;
                }
            }

            return attribute;
        }
    }
}
