using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using RestEase.Platform;

namespace RestEase.Implementation
{
    /// <summary>
    /// Helper class used to generate interface implementations. Exposed for testing (and very adventurous people) only.
    /// </summary>
    public class ImplementationFactory
    {
        private static class TypeCreatorRegistry<T>
        {
            public static Func<IRequester, T>? Creator;
        }

        private readonly object implementationFactoryLockObject = new object();

        // Mapping of generic type definition of interface -> generic type definition of implementation
        // Protected by implementationFactoryLockObject
        private readonly Dictionary<Type, Type> genericTypeLookup = new Dictionary<Type, Type>();

        /// <summary>
        /// Singleton instance of <see cref="ImplementationFactory"/>
        /// </summary>
        public static ImplementationFactory Instance { get; } = new ImplementationFactory();

        private readonly EmitImplementationFactory emitImplementationFactory = new EmitImplementationFactory();

        private ImplementationFactory()
        {
        }

        /// <summary>
        /// Create an implementation of the given interface, using the given requester
        /// </summary>
        /// <typeparam name="T">Type of interface to implement</typeparam>
        /// <param name="requester">Requester to be used by the generated implementation</param>
        /// <returns>An implementation of the given interface</returns>
        public T CreateImplementation<T>(IRequester requester)
        {
            if (requester == null)
                throw new ArgumentNullException(nameof(requester));

            // We have to be careful here. The common case is going to be fetching an existing creator. However in the case
            // that one doesn't yet exist, we can't try and create two of the same type at the same time.
            // We have a lock around creating all types, as that's simpler and probably won't be noticable in practice.

            if (TypeCreatorRegistry<T>.Creator == null)
            {
                lock (this.implementationFactoryLockObject)
                {
                    // Two threads can fail the null test and acquire this lock in order. The first one will create the type.
                    // Therefore the second one has to check for this...
                    if (TypeCreatorRegistry<T>.Creator == null)
                    {
                        try
                        {
                            var implementationType = this.GetImplementation(typeof(T));
                            var creator = this.BuildCreator<T>(implementationType);
                            TypeCreatorRegistry<T>.Creator = creator;
                        }
                        catch (Exception e)
                        {
                            // If they request the same type again, make sure they get the same exception. If we try and build the type
                            // again, they'll get a different exception about a duplicate type.
                            // Yes we nuke the stack trace, but that's not the end of the world since they don't care about our internals
                            TypeCreatorRegistry<T>.Creator = x => throw e;
                            throw;
                        }
                    }
                }
            }

            T implementation = TypeCreatorRegistry<T>.Creator(requester);
            return implementation;
        }

        private Func<IRequester, T> BuildCreator<T>(Type implementationType)
        {
            var requesterParam = Expression.Parameter(typeof(IRequester));
            var ctor = Expression.New(implementationType.GetTypeInfo().GetConstructor(new[] { typeof(IRequester) }), requesterParam);
            return Expression.Lambda<Func<IRequester, T>>(ctor, requesterParam).Compile();
        }

        private Type GetImplementation(Type interfaceType)
        {
            // We're protected by the lock in here
            if (interfaceType.GetTypeInfo().IsGenericType)
            {
                var typeDefinition = interfaceType.GetGenericTypeDefinition();
                if (!this.genericTypeLookup.TryGetValue(typeDefinition, out var implementationTypeDefinition))
                {
                    implementationTypeDefinition = this.BuildImplementation(typeDefinition);
                    this.genericTypeLookup.Add(typeDefinition, implementationTypeDefinition);
                }

                return implementationTypeDefinition.MakeGenericType(interfaceType.GenericTypeArguments);
            }
            else
            {
                return this.BuildImplementation(interfaceType);
            }
        }

        private Type BuildImplementation(Type interfaceType)
        {
            return this.emitImplementationFactory.BuildEmitImplementation(interfaceType);
        }
    }
}
