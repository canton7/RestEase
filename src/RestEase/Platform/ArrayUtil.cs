using System;

namespace RestEase.Platform
{
    internal static class ArrayUtil
    {
#if NET452 || NETSTANDARD1_1
        private static class EmptyArrayCache<T>
        {
            public static readonly T[] Instance = new T[0];
        }

        public static T[] Empty<T>() => EmptyArrayCache<T>.Instance;
#else
        public static T[] Empty<T>() => Array.Empty<T>();
#endif
    }
}
