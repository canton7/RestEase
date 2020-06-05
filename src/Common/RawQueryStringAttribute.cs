using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as being a raw query string, which is inserted as-is into the query string
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class RawQueryStringAttribute : Attribute
    {
    }
}
