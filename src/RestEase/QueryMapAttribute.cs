using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as being the method's Query Map
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class QueryMapAttribute : Attribute
    {
    }
}
