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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A resolver that operates in context of a field input value that can
    /// generate a qualified .NET object for the provided scalar data.
    /// </summary>
    internal class ScalarInputValueResolver : InputValueResolverBase, IInputValueResolver
    {
        private readonly ILeafValueResolver _scalarResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarInputValueResolver" /> class.
        /// </summary>
        /// <param name="scalarResolver">The resolver to resolve a scalar leaf value.</param>
        /// <param name="defaultValueProvider">A provider that can supply a default value when such value is warranted
        /// during value resolution.</param>
        public ScalarInputValueResolver(ILeafValueResolver scalarResolver, IDefaultValueSchemaItem defaultValueProvider)
            : base(defaultValueProvider)
        {
            _scalarResolver = Validation.ThrowIfNullOrReturn(scalarResolver, nameof(scalarResolver));
        }

        /// <inheritdoc />
        public virtual object Resolve(IResolvableValueItem resolvableItem, IResolvedVariableCollection variableData = null)
        {
            if (this.TryResolveViaCommonMethods(resolvableItem, variableData, out var resolvedValue))
                return resolvedValue;

            if (resolvableItem is IResolvableValue resolvableValue)
                return _scalarResolver.Resolve(resolvableValue.ResolvableValue);

            throw this.CreateUnresolvableValueException(resolvableItem);
        }
    }
}