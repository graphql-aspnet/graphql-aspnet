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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A metadata object containing parsed and computed values related to a single parameter
    /// on a C# method that is used a a resolver to a graph field.
    /// </summary>
    internal class FieldResolverParameterMetaData : IGraphFieldResolverParameterMetaData
    {
        public FieldResolverParameterMetaData(
            ParameterInfo paramInfo,
            string internalName,
            string internalFullName,
            GraphArgumentModifiers modifiers,
            object defaultValue = null)
        {
            this.ParameterInfo = Validation.ThrowIfNullOrReturn(paramInfo, nameof(paramInfo));
            this.ExpectedType = this.ParameterInfo.ParameterType;
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));
            this.InternalFullName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalFullName, nameof(internalFullName));
            this.DefaultValue = defaultValue;
            this.ArgumentModifiers = modifiers;
        }

        /// <inheritdoc />
        public ParameterInfo ParameterInfo { get; }

        /// <inheritdoc />
        public string InternalFullName { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public GraphArgumentModifiers ArgumentModifiers { get; }

        /// <inheritdoc />
        public object DefaultValue { get; }

        /// <inheritdoc />
        public Type ExpectedType { get; }
    }
}