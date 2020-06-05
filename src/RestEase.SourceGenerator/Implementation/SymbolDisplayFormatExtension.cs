using Microsoft.CodeAnalysis;

namespace RestEase.SourceGenerator.Implementation
{
    internal static class SymbolDisplayFormatExtensions
    {
        public static SymbolDisplayFormat WithTypeQualificationStyle(this SymbolDisplayFormat @this, SymbolDisplayTypeQualificationStyle typeQualificationStyle)
        {
            return new SymbolDisplayFormat(
                @this.GlobalNamespaceStyle,
                typeQualificationStyle,
                @this.GenericsOptions,
                @this.MemberOptions,
                @this.DelegateStyle,
                @this.ExtensionMethodStyle,
                @this.ParameterOptions,
                @this.PropertyStyle,
                @this.LocalOptions,
                @this.KindOptions,
                @this.MiscellaneousOptions);
        }
    }
}
