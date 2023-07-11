// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet
{
    using System;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A global set of providers used throughout GraphQL.AspNet. These objects are static, unchanged and expected to
    /// not change at runtime. Do not alter the contents of the static properties after calling <c>.AddGraphQL()</c>.
    /// </summary>
    internal static class GraphQLProviders
    {
        /// <summary>
        /// attempts to instnatiate the provided type as a union proxy.
        /// </summary>
        /// <param name="proxyType">Type of the proxy to create.</param>
        /// <returns>IGraphUnionProxy.</returns>
        public static IGraphUnionProxy CreateUnionProxyFromType(Type proxyType)
        {
            if (proxyType == null)
                return null;

            IGraphUnionProxy proxy = null;
            if (Validation.IsCastable<IGraphUnionProxy>(proxyType))
            {
                var paramlessConstructor = proxyType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
                if (paramlessConstructor == null)
                {
                    throw new GraphTypeDeclarationException(
                        $"The union proxy type '{proxyType.FriendlyName()}' could not be instantiated. " +
                        "All union proxy types must declare a parameterless constructor.");
                }

                proxy = InstanceFactory.CreateInstance(proxyType) as IGraphUnionProxy;
            }

            return proxy;
        }
    }
}