using Microsoft.CodeAnalysis;

namespace RestEase.SourceGenerator.Implementation
{
    internal static class RoslynEmitUtils
    {
        public static string QuoteString(string? s) => s == null ? "null" : "@\"" + s.Replace("\"", "\"\"") + "\"";

        private static readonly SymbolDisplayFormat genericTypeofFormat = SymbolDisplayFormats.TypeofParameter
            .WithGenericsOptions(SymbolDisplayGenericsOptions.None);

        public static string AddBareAngles(INamedTypeSymbol symbol, string name)
        {
            return symbol.IsGenericType
                ? name + "<" + new string(',', symbol.TypeParameters.Length - 1) + ">"
                : name;
        }

        public static string GetGenericTypeDefinitionTypeof(INamedTypeSymbol symbol, SymbolDisplayTypeQualificationStyle? typeQualificationStyle = null)
        {
            var format = genericTypeofFormat;
            if (typeQualificationStyle != null && typeQualificationStyle != format.TypeQualificationStyle)
            {
                format = format.WithTypeQualificationStyle(typeQualificationStyle.Value);
            }

            string name = symbol.ToDisplayString(format);
            return symbol.IsGenericType
                    ? name + "<" + new string(',', symbol.TypeParameters.Length - 1) + ">"
                    : name;
        }
    }
}
