// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Resolvers
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A metadata object containing parsed and computed values related to
    /// C# method that is used a a resolver to a graph field.
    /// </summary>
    [DebuggerDisplay("Method: {InternalName}")]
    internal class FieldResolverMetaData : IGraphFieldResolverMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldResolverMetaData"/> class.
        /// </summary>
        /// <param name="method">The method info will be invoked to fulfill the resolver.</param>
        /// <param name="parameters">The parameters metadata collection related to this resolver method.</param>
        /// <param name="expectedReturnType">Expected type of the data to be returned by the method. May be different
        /// from concrete return types (e.g. expecting an interface but actually returning a concrete type that implements that interface).</param>
        /// <param name="isAsyncField">if set to <c>true</c> the invoked method is asyncronous.</param>
        /// <param name="internalName">The name of the resolver method or property as it exists in source code.</param>
        /// <param name="internalFullName">the full name of the resolver method or propert, with namespace and parent owning class,
        /// as it exists in source code.</param>
        /// <param name="parentObjectType">The type of the .NET class or struct where the resolver method is declared.</param>
        /// <param name="parentInternalName">The name of the .NET class or struct where the resolver method is declared.</param>
        /// <param name="parentInternalFullName">The full name of the .NET class or struct, including namespace, where the resolver method is declared.</param>
        public FieldResolverMetaData(
            MethodInfo method,
            IGraphFieldResolverParameterMetaDataCollection parameters,
            Type expectedReturnType,
            bool isAsyncField,
            string internalName,
            string internalFullName,
            Type parentObjectType,
            string parentInternalName,
            string parentInternalFullName)
        {
            this.Method = Validation.ThrowIfNullOrReturn(method, nameof(method));

            this.ExpectedReturnType = Validation.ThrowIfNullOrReturn(expectedReturnType, nameof(expectedReturnType));

            this.IsAsyncField = isAsyncField;
            this.Parameters = Validation.ThrowIfNullOrReturn(parameters, nameof(parameters));

            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));
            this.InternalFullName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalFullName, nameof(internalFullName));

            this.ParentObjectType = Validation.ThrowIfNullOrReturn(parentObjectType, nameof(parentObjectType));
            this.ParentInternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(parentInternalName, nameof(parentInternalName));
            this.ParentInternalFullName = Validation.ThrowIfNullWhiteSpaceOrReturn(parentInternalFullName, nameof(parentInternalFullName));
        }

        /// <inheritdoc />
        public Type ExpectedReturnType { get; }

        /// <inheritdoc />
        public MethodInfo Method { get; }

        /// <inheritdoc />
        public bool IsAsyncField { get; }

        /// <inheritdoc />
        public string InternalFullName { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public IGraphFieldResolverParameterMetaDataCollection Parameters { get; }

        /// <inheritdoc />
        public string ParentInternalName { get; }

        /// <inheritdoc />
        public string ParentInternalFullName { get; }

        /// <inheritdoc />
        public Type ParentObjectType { get; }
    }
}