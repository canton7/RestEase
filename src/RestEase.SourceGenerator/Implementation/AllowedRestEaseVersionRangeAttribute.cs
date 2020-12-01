using System;

namespace RestEase.SourceGenerator.Implementation
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    internal class AllowedRestEaseVersionRangeAttribute : Attribute
    {
        public string MinVersionInclusive { get; }
        public string MaxVersionExclusive { get; }

        public AllowedRestEaseVersionRangeAttribute(string minVersionInclusive, string maxVersionExlusive)
        {
            this.MinVersionInclusive = minVersionInclusive;
            this.MaxVersionExclusive = maxVersionExlusive;
        }
    }
}
