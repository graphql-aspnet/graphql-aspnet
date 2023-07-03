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
            IEnumerable<ParameterInfo> parameters,
            IEnumerable<IGraphArgumentTemplate> arguments,
            Type expectedReturnType,
            Type objectType,
            SchemaItemPath route,
            bool isAsyncField,
            string name,
            string internalName,
            string internalFullName,
            Type parentObjectType,
            string parentInternalName,
            string parentInternalFullName)
        {
            this.Method = Validation.ThrowIfNullOrReturn(method, nameof(method));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));

            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.ExpectedReturnType = Validation.ThrowIfNullOrReturn(expectedReturnType, nameof(expectedReturnType));

            this.IsAsyncField = isAsyncField;
            this.Parameters = new List<ParameterInfo>(parameters ?? Enumerable.Empty<ParameterInfo>());
            this.Arguments = new List<IGraphArgumentTemplate>(arguments ?? Enumerable.Empty<IGraphArgumentTemplate>());

            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
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
        public string Name { get; }

        /// <inheritdoc />
        public string InternalFullName { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public IReadOnlyList<ParameterInfo> Parameters { get; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; }

        /// <inheritdoc />
        public IReadOnlyList<IGraphArgumentTemplate> Arguments { get; }

        /// <inheritdoc />
        public string ParentInternalName { get; }

        /// <inheritdoc />
        public string ParentInternalFullName { get; }

        /// <inheritdoc />
        public Type ParentObjectType { get; }
    }
}