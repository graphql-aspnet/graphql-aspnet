// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Resolvers
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
        /// Initializes a new instance of the <see cref="FieldResolverMetaData" /> class.
        /// </summary>
        /// <param name="method">The method info will be invoked to fulfill the resolver.</param>
        /// <param name="parameters">The parameters metadata collection related to this resolver method.</param>
        /// <param name="expectedReturnType">Expected type of the data to be returned by the method. May be different
        /// from concrete return types (e.g. expecting an interface but actually returning a concrete type that implements that interface).</param>
        /// <param name="isAsyncField">if set to <c>true</c> the invoked method is asyncronous.</param>
        /// <param name="internalName">The internal name of the resolver method or property that can uniquely identify it in
        /// exceptions and log entries.</param>
        /// <param name="declaredName">The exact name of the resolver method or property name as its declared in source code.</param>
        /// <param name="parentObjectType">The type of the .NET class or struct where the resolver method is declared.</param>
        /// <param name="parentInternalName">The name of the .NET class or struct where the resolver method is declared.</param>
        public FieldResolverMetaData(
            MethodInfo method,
            IGraphFieldResolverParameterMetaDataCollection parameters,
            Type expectedReturnType,
            bool isAsyncField,
            string internalName,
            string declaredName,
            Type parentObjectType,
            string parentInternalName)
        {
            this.Method = Validation.ThrowIfNullOrReturn(method, nameof(method));

            this.ExpectedReturnType = Validation.ThrowIfNullOrReturn(expectedReturnType, nameof(expectedReturnType));

            this.IsAsyncField = isAsyncField;
            this.Parameters = Validation.ThrowIfNullOrReturn(parameters, nameof(parameters));

            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));
            this.DeclaredName = Validation.ThrowIfNullWhiteSpaceOrReturn(declaredName, nameof(declaredName));
            this.ParentObjectType = Validation.ThrowIfNullOrReturn(parentObjectType, nameof(parentObjectType));
            this.ParentInternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(parentInternalName, nameof(parentInternalName));
        }

        /// <inheritdoc />
        public Type ExpectedReturnType { get; }

        /// <inheritdoc />
        public MethodInfo Method { get; }

        /// <inheritdoc />
        public bool IsAsyncField { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public IGraphFieldResolverParameterMetaDataCollection Parameters { get; }

        /// <inheritdoc />
        public string ParentInternalName { get; }

        /// <inheritdoc />
        public Type ParentObjectType { get; }

        /// <inheritdoc />
        public string DeclaredName { get; }
    }
}