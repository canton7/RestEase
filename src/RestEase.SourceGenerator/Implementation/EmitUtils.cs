using System;
using System.Collections.Generic;
using System.Text;

namespace RestEase.SourceGenerator.Implementation
{
    internal static class EmitUtils
    {
        public static string QuoteString(string? s) => s == null ? "null" : "@\"" + s.Replace("\"", "\"\"") + "\"";
    }
}
