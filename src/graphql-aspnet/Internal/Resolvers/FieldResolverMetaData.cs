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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A concrete class that implements <see cref="IGraphFieldResolverMetaData"/>. Used by the templating
    /// engine to generate invocation info for any method or property.
    /// </summary>
    internal class FieldResolverMetaData : IGraphFieldResolverMetaData
    {
        public FieldResolverMetaData(
            MethodInfo method,
            IGraphFieldResolverParameterMetaDataCollection parameters,
            Type expectedReturnType,
            Type objectType,
            bool isAsyncField,
            string internalName,
            string internalFullName,
            Type parentObjectType,
            string parentInternalName,
            string parentInternalFullName)
        {
            this.Method = Validation.ThrowIfNullOrReturn(method, nameof(method));

            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
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
        public Type ObjectType { get; }

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