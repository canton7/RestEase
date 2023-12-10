using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RestEase.Platform;

namespace RestEase.Implementation
{
    internal class ImplementationFactory
    {
        private static class TypeCreatorRegistry<T>
        {
            public static Func<IRequester, T>? Creator;
        }

        private readonly object implementationFactoryLockObject = new();

        // Mapping of generic type definition of interface -> generic type definition of implementation
        // Protected by implementationFactoryLockObject
        private readonly Dictionary<Type, Type> genericTypeLookup = new();

        private readonly bool useSourceGenerator;

        public ImplementationFactory(bool useSourceGenerator = true)
        {
            this.useSourceGenerator = useSourceGenerator;
        }

        public T CreateImplementation<T>(IRequester requester)
        {
            if (requester == null)
                throw new ArgumentNullException(nameof(requester));

            // We have to be careful here. The common case is going to be fetching an existing creator. However in the case
            // that one doesn't yet exist, we can't try and create two of the same type at the same time.
            // We have a lock around creating all types, as that's simpler and probably won't be noticeable in practice.

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
                            var creator = BuildCreator<T>(implementationType);
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

        private static Func<IRequester, T> BuildCreator<T>(Type implementationType)
        {
            var requesterParam = Expression.Parameter(typeof(IRequester));
            var constructorInfo = implementationType.GetTypeInfo().GetConstructor(new[] { typeof(IRequester) });
            if (constructorInfo == null)
            {
                throw new ImplementationCreationException($"Could not find a suitable constructor on type {implementationType}. This should not happen");
            }

            var constructor = Expression.New(constructorInfo, requesterParam);
            return Expression.Lambda<Func<IRequester, T>>(constructor, requesterParam).Compile();
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
            // interfaceType is either a non-generic type or a generic type definition

            if (this.useSourceGenerator)
            {
                var attributes = interfaceType.GetTypeInfo().Assembly.GetCustomAttributes<RestEaseInterfaceImplementationAttribute>();
                var implementationType = attributes.FirstOrDefault(x => x.InterfaceType == interfaceType)?.ImplementationType;
                if (implementationType != null)
                {
                    return implementationType;
                }
            }

            try
            {
                return EmitImplementationFactory.Instance.BuildEmitImplementation(interfaceType);
            }
            catch (PlatformNotSupportedException e)
            {
                throw new ImplementationCreationException($"This platform does not support runtime code generation, and RestEase " +
                    "was unable to find an implementation of '{interfaceType.FullName}' generated by RestEase.SourceGenerator. " +
                    "Please make sure that the RestEase.SourceGenerator NuGet package is installed in the assembly which contains " +
                    $"'{interfaceType.FullName}' ('{interfaceType.GetTypeInfo().Assembly.FullName}').", e);
            }
        }
    }
}
