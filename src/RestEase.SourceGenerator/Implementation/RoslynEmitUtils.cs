using Microsoft.CodeAnalysis;

namespace RestEase.SourceGenerator.Implementation
{
    internal static class RoslynEmitUtils
    {
        public static string QuoteString(string? s) => s == null ? "null" : "@\"" + s.Replace("\"", "\"\"") + "\"";

        public static string AddBareAngles(INamedTypeSymbol symbol, string name)
        {
            return symbol.IsGenericType
                ? name + "<" + new string(',', symbol.TypeParameters.Length - 1) + ">"
                : name;
        }
    }
}
