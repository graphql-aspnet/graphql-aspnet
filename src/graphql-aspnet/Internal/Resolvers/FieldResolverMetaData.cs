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
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A metadata object containing parsed and computed values related to
    /// C# method that is used a a resolver to a graph field.
    /// </summary>
    internal class FieldResolverMetaData : IGraphFieldResolverMetaData
    {
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