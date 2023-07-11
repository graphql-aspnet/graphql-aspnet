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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A metadata object containing parsed and computed values related to a single parameter
    /// on a C# method that is used a a resolver to a graph field.
    /// </summary>
    [DebuggerDisplay("Parameter: {InternalName}")]
    internal class FieldResolverParameterMetaData : IGraphFieldResolverParameterMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldResolverParameterMetaData" /> class.
        /// </summary>
        /// <param name="paramInfo">The parameter info for a single parameter within a resolver method.</param>
        /// <param name="internalName">The name of the parameter as its declared in source code.</param>
        /// <param name="internalFullName">The full name of the parameter, including namespace, owning object and declared method, as it exists in source code.</param>
        /// <param name="modifiers">Any modifier attributes for this parameter discovered via templating or set
        /// at runtime by the target schema.</param>
        /// <param name="isListBasedParameter">if set to <c>true</c> this parameter is expecting a list
        /// of items to be passed to it at runtime.</param>
        /// <param name="defaultValue">The default value assigned to this parameter in source code when the parameter
        /// was declared.</param>
        public FieldResolverParameterMetaData(
            ParameterInfo paramInfo,
            string internalName,
            string internalFullName,
            GraphArgumentModifiers modifiers,
            bool isListBasedParameter,
            object defaultValue = null)
        {
            this.ParameterInfo = Validation.ThrowIfNullOrReturn(paramInfo, nameof(paramInfo));
            this.ExpectedType = this.ParameterInfo.ParameterType;
            this.UnwrappedExpectedParameterType = GraphValidation.EliminateWrappersFromCoreType(
                this.ExpectedType,
                eliminateEnumerables: true,
                eliminateTask: true,
                eliminateNullableT: false);

            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));
            this.InternalFullName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalFullName, nameof(internalFullName));
            this.DefaultValue = defaultValue;
            this.ArgumentModifiers = modifiers;
            this.IsList = isListBasedParameter;
        }

        /// <inheritdoc />
        public ParameterInfo ParameterInfo { get; }

        /// <inheritdoc />
        public string InternalFullName { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public GraphArgumentModifiers ArgumentModifiers { get; private set; }

        /// <inheritdoc />
        public object DefaultValue { get; }

        /// <inheritdoc />
        public Type ExpectedType { get; }

        /// <inheritdoc />
        public Type UnwrappedExpectedParameterType { get; }

        /// <inheritdoc />
        public bool IsList { get; }
    }
}