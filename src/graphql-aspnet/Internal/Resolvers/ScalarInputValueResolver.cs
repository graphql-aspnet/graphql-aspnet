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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A resolver that operates in context of a field input value that can
    /// generate a qualified .NET object for the provided scalar data.
    /// </summary>
    internal class ScalarInputValueResolver : IInputValueResolver
    {
        private readonly ILeafValueResolver _scalarResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarInputValueResolver" /> class.
        /// </summary>
        /// <param name="scalarResolver">The resolver to resolve a scalar leaf value.</param>
        public ScalarInputValueResolver(ILeafValueResolver scalarResolver)
        {
            _scalarResolver = Validation.ThrowIfNullOrReturn(scalarResolver, nameof(scalarResolver));
        }

        /// <inheritdoc />
        public virtual object Resolve(IResolvableValueItem resolvableItem, IResolvedVariableCollection variableData = null)
        {
            if (resolvableItem is IResolvablePointer pointer)
            {
                IResolvedVariable variable = null;
                var variableFound = variableData?.TryGetValue(pointer.PointsTo, out variable) ?? false;
                if (variableFound)
                    return variable.Value;
            }

            if (resolvableItem is IResolvableValue resolvableValue)
                return _scalarResolver.Resolve(resolvableValue.ResolvableValue);

            return null;
        }
    }
}