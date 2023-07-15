// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A map of .NET types and their related built in scalar types and unions.
    /// </summary>
    public static partial class GlobalTypes
    {
        /// <summary>
        /// Validates that the type can be instantiated as a union proxy. Throws an exception if it cannot.
        /// </summary>
        /// <param name="proxyType">The Type of the proxy to test.</param>
        public static void ValidateUnionProxyOrThrow(Type proxyType)
        {
            CreateAndValidateUnionProxyType(proxyType, true);
        }

        /// <summary>
        /// Validates that the provided union proxy is valid. If it is not, an exception is thrown.
        /// </summary>
        /// <param name="unionProxy">The union proxy instance to test.</param>
        public static void ValidateUnionProxyOrThrow(IGraphUnionProxy unionProxy)
        {
            ValidateUnionProxy(unionProxy, true);
        }

        /// <summary>
        /// Validates that the union proxy type provided is usable as a union within an object graph.
        /// </summary>
        /// <param name="proxyType">The proxy type to validate.</param>
        /// <param name="shouldThrow">if set to <c>true</c> this method should an exception when
        /// an invalid proxy is checked. If set to <c>false</c> then <c>false</c> is returned when an invalid proxy is checked.</param>
        /// <returns><c>true</c> if the proxy is valid, <c>false</c> otherwise.</returns>
        private static (bool IsValid, IGraphUnionProxy Instance) CreateAndValidateUnionProxyType(Type proxyType, bool shouldThrow)
        {
            if (proxyType == null)
            {
                if (!shouldThrow)
                    return (false, null);

                throw new GraphTypeDeclarationException("~null~ is an invalid union proxy type.");
            }

            if (!Validation.IsCastable<IGraphUnionProxy>(proxyType))
            {
                if (!shouldThrow)
                    return (false, null);

                throw new GraphTypeDeclarationException(
                    $"The type {proxyType.FriendlyGraphTypeName()} does not implement {nameof(IGraphUnionProxy)}. All " +
                    $"types being used as a declaration of a union must implement {nameof(IGraphUnionProxy)}.");
            }

            var paramlessConstructor = proxyType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            if (paramlessConstructor == null)
            {
                if (!shouldThrow)
                    return (false, null);
                throw new GraphTypeDeclarationException(
                    $"The union proxy type '{proxyType.FriendlyName()}' could not be instantiated. " +
                    "All union proxy types must declare a parameterless constructor.");
            }

            var proxy = InstanceFactory.CreateInstance(proxyType) as IGraphUnionProxy;
            return ValidateUnionProxy(proxy, shouldThrow);
        }

        private static (bool IsValid, IGraphUnionProxy Instance) ValidateUnionProxy(IGraphUnionProxy proxy, bool shouldThrow)
        {
            Validation.ThrowIfNull(proxy, nameof(proxy));

            if (!GraphValidation.IsValidGraphName(proxy.Name))
            {
                if (!shouldThrow)
                    return (false, null);
                throw new GraphTypeDeclarationException(
                    $"The union proxy {proxy.GetType().FriendlyName()} must supply a name that that conforms to the standard rules for GraphQL. (Regex: {Constants.RegExPatterns.NameRegex})");
            }

            if (proxy.Types == null || proxy.Types.Count < 1)
            {
                if (!shouldThrow)
                    return (false, null);

                throw new GraphTypeDeclarationException(
                    $"The union proxy {proxy.GetType().FriendlyName()} must declare at least one valid Type to be a part of the union.");
            }

            return (true, proxy);
        }

        /// <summary>
        /// Determines whether the provided type represents an object that is a properly constructed scalar graph type.
        /// </summary>
        /// <param name="typeToCheck">The type to check.</param>
        /// <returns><c>true</c> if the type represents a valid scalar; otherwise, <c>false</c>.</returns>
        public static bool IsValidUnionProxyType(Type typeToCheck)
        {
            return CreateAndValidateUnionProxyType(typeToCheck, false).IsValid;
        }

        /// <summary>
        /// Attempts to instnatiate the provided type as a union proxy. If the proxy type is invalid, null is returned.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ValidateUnionProxyOrThrow(Type)"/> to check for correctness.
        /// </remarks>
        /// <param name="proxyType">Type of the proxy to create.</param>
        /// <returns>IGraphUnionProxy.</returns>
        public static IGraphUnionProxy CreateUnionProxyFromType(Type proxyType)
        {
            var (isValid, result) = CreateAndValidateUnionProxyType(proxyType, false);
            return isValid ? result : null;
        }
    }
}